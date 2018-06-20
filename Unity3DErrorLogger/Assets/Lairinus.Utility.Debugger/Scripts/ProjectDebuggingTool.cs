using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using System.Collections;
using System.Linq;

namespace Lairinus.Utility
{
    public class ProjectDebuggingTool : MonoBehaviour
    {
        private static bool _initializingPrefab = false;

        private static bool _isInitialized = false; // Determines if we are currently working on initializing the Prefab object

        private Dictionary<string, ProjectDebuggingItemModel> _allDebuggingModelItems = new Dictionary<string, ProjectDebuggingItemModel>(); // The actual data for the Debugging Items

        private Dictionary<string, ProjectDebuggingItemView> _allDebuggingViewItems = new Dictionary<string, ProjectDebuggingItemView>(); // The UI representation of the Debugging Items

        [SerializeField] private bool _alwaysShowLogButton = false; // If true, the log button will always appear. If this is false, you will need to use UIDebuggingConsoleTool.Show() and UIDebuggingConsoleTool.Close()
        private string _currentlyShownPageKey = ""; // The page that is being shown

        private ProjectDebuggingItemModel _currentlyViewedItem = null; // The DebuggingItem that is currently being inspected

        private List<ProjectDebuggingItemView> _displayedUIDebuggingItems = new List<ProjectDebuggingItemView>(); // The UIDebuggingItems that are currently being shown. This list is adjusted to fit the current filters

        [SerializeField] private ErrorDetailPageElements _errorDetailPage = new ErrorDetailPageElements(); // Internal use only

        // Internal use only

        [SerializeField] private PageNavigation _navigation = new PageNavigation(); // Internal use only

        [SerializeField] private Options _options = new Options(); // Internal use only

        [SerializeField] private ProjectDebuggingStyle_ScriptableObject _styleSO = null; // Internal use only

        [SerializeField] private ProjectDebuggingItemView _uiDebuggingItemPrefab = null; // Internal use only

        [SerializeField] private Transform _uiDebuggingItemTransformParent = null; // Internal use only

        /// <summary>
        /// Returns true if there is at least one DebuggingItem that exists. This does not adjust for the current filters
        /// </summary>
        public bool debuggingItemsExist { get { return _allDebuggingModelItems.Values.Count > 0; } }

        /// <summary>
        /// Returns true if there is at least one DebuggingItem shown. This is adjusted for the visibility filters
        /// </summary>
        public bool filteredDebuggingItemsExist
        {
            get { return _displayedUIDebuggingItems.Count > 0; }
        }

        private static ProjectDebuggingTool _instance { get; set; }

        /// <summary>
        /// Adds a Log Item to the Debugging Console
        /// </summary>
        /// <param name="name">name of the Log Item</param>
        /// <param name="stackTrace">Where the log item occurred</param>
        /// <param name="type">Which Log Type is this item, based on Unity's defaults?</param>
        public static void AddLogItem(string name, string stackTrace, LogType type, MonoBehaviour callingObject = null)
        {
            if (_instance == null)
            {
                if (callingObject == null)
                {
                    Debug.LogError("Error: Lairinus.Utility.UIDebuggingConsoleTool\n You need to instantiate the UIDebuggingConsoleTool before trying to use it! If you don't know if this object will be instantiated, add the parameter 'callingObject' as a fallback.");
                    return;
                }
                callingObject.StartCoroutine(AddIssueItemRoutine(name, stackTrace, type));
            }
            else
                _instance.ProcessAddLogItem_Internal(name, stackTrace, type);
        }

        /// <summary>
        /// Internal use only. Internally closes the Debugging tool
        /// </summary>
        public static void Close()
        {
            if (_instance == null)
                return;

            _instance._navigation.mainPage.SetActive(false);
            _instance._navigation.optionsPage.SetActive(false);
            _instance._navigation.errorDetailsPage.SetActive(false);
        }

        /// <summary>
        /// Initializes the Debugging Log. A MonoBehaviour component is required to process a Coroutine. If this is not possible, then drag the Lairinus.ProjectDebuggingTool tool into the scene and you won't have to use this method :)
        /// </summary>
        /// <param name="callingObject"></param>
        public static void Initialize(MonoBehaviour callingObject)
        {
            if (callingObject == null)
            {
                Debug.LogError(Strings.Error_RequireMonobehaviour);
                return;
            }
            callingObject.StartCoroutine(InitRoutine());
        }

        /// <summary>
        /// Manually shows the DebuggingLog tool
        /// </summary>
        public static void Show()
        {
            if (_instance == null)
            {
                Debug.LogError(Strings.Error_InitializeBeforeUse);
                TryCreateSingletonObject();
                return;
            }

            _instance.OnClick_ShowPage(Strings.mainPageKey);
        }

        /// <summary>
        /// Internal use only. Provides a timeout between adding a DebuggingItem and creating a new instance of the class if it doesn't exist in the scene
        /// </summary>
        /// <param name="name"></param>
        /// <param name="stackTrace"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private static IEnumerator AddIssueItemRoutine(string name, string stackTrace, LogType type)
        {
            if (!_initializingPrefab)
            {
                TryCreateSingletonObject();

                while (!_isInitialized)
                {
                    yield return null;
                }
                _instance.ProcessAddLogItem_Internal(name, stackTrace, type);
            }

            // Wait until this object is initialized
            else
            {
                while (!_isInitialized && _instance == null)
                {
                    yield return null;
                }

                _instance.ProcessAddLogItem_Internal(name, stackTrace, type);
            }
        }

        private static IEnumerator InitRoutine()
        {
            if (_initializingPrefab)
            {
                _initializingPrefab = true;
                TryCreateSingletonObject();
                while (!_isInitialized)
                {
                    yield return null;
                }
            }
        }

        private static void TryCreateSingletonObject()
        {
            if (_instance == null && !_initializingPrefab)
            {
                _initializingPrefab = true;
                Instantiate(Resources.Load(Strings.debuggingToolPrefabName));
            }
        }

        /// <summary>
        /// Internal use only. Attaches to Unity's event handler and catches any errors that Unity encounters
        /// </summary>
        /// <param name="name"></param>
        /// <param name="stackTrace"></param>
        /// <param name="type"></param>
        private void HandleException(string name, string stackTrace, LogType type)
        {
            ProcessAddLogItem_Internal(name, stackTrace, type);
        }

        /// <summary>
        /// By this point, all items are already in the _displayed list, so we just need to show/hide the items from there.
        /// </summary>
        private void HandleOnToggle_RecalculateShownValues_Internal()
        {
            _displayedUIDebuggingItems.ForEach(x => x.gameObject.SetActive(false));
            _displayedUIDebuggingItems = new List<ProjectDebuggingItemView>();
            foreach (KeyValuePair<string, ProjectDebuggingItemView> kvp in _allDebuggingViewItems)
            {
                ProjectDebuggingItemView item = kvp.Value;
                if (item == null)
                    continue;

                // We only want to show the View object if the object is allowed to be shown.
                if (_options.CanShowType(item.type))
                    _displayedUIDebuggingItems.Add(item);
            }

            _displayedUIDebuggingItems.ForEach(x => x.gameObject.SetActive(true));
        }

        /// <summary>
        /// Internal use only
        /// </summary>
        private void InitializeInternal()
        {
            if (_instance != null && _instance != this)
                Destroy(gameObject);

            try
            {
                if (!_isInitialized)
                {
                    _isInitialized = true;
                    _instance = this;
                    Application.logMessageReceived -= HandleException;
                    Application.logMessageReceived += HandleException;

                    if (_navigation.showLogButton != null)
                        _navigation.showLogButton.onClick.AddListener(() => OnClick_ShowPage(Strings.mainPageKey));

                    if (_navigation.showMainConsoleButton != null)
                        _navigation.showMainConsoleButton.onClick.AddListener(() => OnClick_ShowPage(Strings.mainPageKey));

                    if (_navigation.showOptionsButton != null)
                        _navigation.showOptionsButton.onClick.AddListener(() => OnClick_ShowPage(Strings.optionsPageKey));

                    if (_navigation.exitConsoleButton != null)
                        _navigation.exitConsoleButton.onClick.AddListener(() => Close());

                    if (_navigation.backToMainFromLogButton != null)
                        _navigation.backToMainFromLogButton.onClick.AddListener(() => OnClick_ShowPage(Strings.mainPageKey));

                    if (_options != null)
                        _options.AddEventListeners(() => HandleOnToggle_RecalculateShownValues_Internal());
                }
            }
            catch
            {
                // Do nothing, because we can't do anything. The debugging tool has a user error.
                Debug.LogError(Strings.Error_PackageIssue);
            }
        }

        /// <summary>
        /// Shows the details for a DebuggingItem once it is clicked
        /// </summary>
        /// <param name="item"></param>
        private void OnClick_ShowDebuggingItemLogDetails(ProjectDebuggingItemModel item)
        {
            _currentlyViewedItem = item;
            OnClick_ShowPage(Strings.errorDetailsPageKey);
        }

        // Prepares and then shows the page
        private void OnClick_ShowPage(string pageName)
        {
            if (_instance == null)
            {
                TryCreateSingletonObject();
                return;
            }

            _currentlyShownPageKey = pageName;
            switch (pageName)
            {
                case Strings.mainPageKey:
                    {
                        StopCoroutine(Strings.updateErrorLogRoutine);
                        _navigation.mainPage.SetActive(true);
                        _navigation.optionsPage.SetActive(false);
                        _navigation.errorDetailsPage.SetActive(false);
                    }
                    break;

                case Strings.optionsPageKey:
                    {
                        StopCoroutine(Strings.updateErrorLogRoutine);
                        _navigation.mainPage.SetActive(false);
                        _navigation.optionsPage.SetActive(true);
                        _navigation.errorDetailsPage.SetActive(false);
                    }
                    break;

                case Strings.errorDetailsPageKey:
                    {
                        StartCoroutine(Strings.updateErrorLogRoutine);
                        _navigation.mainPage.SetActive(false);
                        _navigation.optionsPage.SetActive(false);
                        _navigation.errorDetailsPage.SetActive(true);
                    }
                    break;
            }
        }

        /// <summary>
        /// Internal use only. While inactive, this object does not catch errors
        /// </summary>
        private void OnDisable()
        {
            Application.logMessageReceived -= HandleException;
        }

        /// <summary>
        /// Internal use only. While active, this object catches all errors in Unity3D
        /// </summary>
        private void OnEnable()
        {
            Application.logMessageReceived -= HandleException;
            Application.logMessageReceived += HandleException;
        }

        /// <summary>
        /// Internal use only. Processes adding the item after we're sure we have a Singleton to use. Internal use only.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="stackTrace"></param>
        /// <param name="type"></param>
        private void ProcessAddLogItem_Internal(string name, string stackTrace, LogType type)
        {
            if (_instance == null)
                return;

            // Creates the Model and prepares it for use
            string key = name + stackTrace;
            ProjectDebuggingItemModel item = null;
            if (_instance._allDebuggingModelItems.ContainsKey(key))
                item = _instance._allDebuggingModelItems[key];
            else
            {
                item = new ProjectDebuggingItemModel(name, stackTrace, type);
                _instance._allDebuggingModelItems.Add(key, item);
            }

            item.lastOccurence = Time.realtimeSinceStartup;
            item.lastOccurenceLocalTime = DateTime.Now.ToShortTimeString();
            item.count++;

            // Creates the View (if needed) and applies the model to it
            ProjectDebuggingItemView uiDebuggingItem = null;
            if (_instance._allDebuggingViewItems.ContainsKey(key))
                uiDebuggingItem = _instance._allDebuggingViewItems[key];
            else
            {
                uiDebuggingItem = Instantiate(_instance._uiDebuggingItemPrefab, _instance._uiDebuggingItemTransformParent);
                _instance._allDebuggingViewItems.Add(key, uiDebuggingItem);
            }

            _instance._styleSO.SetUIDebuggingItemStyle(uiDebuggingItem, type);
            uiDebuggingItem.titleText.text = item.name;
            uiDebuggingItem.countText.text = Strings.quantitySeparator + item.count.ToString();
            uiDebuggingItem.backgroundButton.onClick.RemoveAllListeners();
            uiDebuggingItem.type = item.type;
            uiDebuggingItem.backgroundButton.onClick.AddListener(() => _instance.OnClick_ShowDebuggingItemLogDetails(item));

            // Show or hide the View item based on whether or not the type can be shown
            if (_options.CanShowType(item.type))
                uiDebuggingItem.gameObject.SetActive(true);
            else
                uiDebuggingItem.gameObject.SetActive(false);

            // Add the item to the "Displayed items" list.
            _displayedUIDebuggingItems = _allDebuggingViewItems.Values.ToList();

            // Show the 'Show Log' button if applicable
            if (_instance._allDebuggingModelItems.Values.Count > 0 || _alwaysShowLogButton)
                _instance._navigation.showLogButton.gameObject.SetActive(true);
            else
                _instance._navigation.showLogButton.gameObject.SetActive(false);
        }

        /// <summary>
        /// Internal use only. Unity3D's "Start" is like a class constructor
        /// </summary>
        private void Start()
        {
            InitializeInternal();
        }

        /// <summary>
        /// Updates the Error Detail page as long as it is open.
        /// </summary>
        /// <returns></returns>
        private IEnumerator UpdateErrorLogDetailPage_Routine()
        {
            while (true)
            {
                yield return null;
                if (_currentlyViewedItem != null)
                {
                    _errorDetailPage.nameText.text = _currentlyViewedItem.name;
                    _errorDetailPage.countText.text = _currentlyViewedItem.count.ToString();
                    _errorDetailPage.stackTraceText.text = _currentlyViewedItem.stacktrace;
                    _errorDetailPage.lastOccurenceText.text = "Time since play: " + Mathf.Round(_currentlyViewedItem.lastOccurence) + "\n" + "Actual Time: " + _currentlyViewedItem.lastOccurenceLocalTime;
                    _errorDetailPage.typeText.text = _currentlyViewedItem.logTypeText;
                }
            }
        }

        // Contains elements for the Error Details page
        [System.Serializable]
        public class ErrorDetailPageElements
        {
            public Text countText = null;
            public Text lastOccurenceText = null;
            public Text nameText = null;
            public Text stackTraceText = null;
            public Text typeText = null;
        }

        /// <summary>
        /// Utility used to help with how the items display on the Main page of the tool
        /// </summary>
        [System.Serializable]
        public class Options
        {
            [SerializeField] private UIToggle _showAssertionsToggle = null;
            [SerializeField] private UIToggle _showErrorsToggle = null;
            [SerializeField] private UIToggle _showExceptionsToggle = null;
            [SerializeField] private UIToggle _showLogsToggle = null;
            [SerializeField] private UIToggle _showWarningsToggle = null;
            public bool showAssertions { get { return _showAssertionsToggle.isOn; } }
            public bool showErrors { get { return _showErrorsToggle.isOn; } }
            public bool showExceptions { get { return _showExceptionsToggle.isOn; } }
            public bool showLogs { get { return _showLogsToggle.isOn; ; } }
            public bool showWarnings { get { return _showWarningsToggle.isOn; } }

            public void AddEventListeners(Action action)
            {
                _showAssertionsToggle.onToggle = action;
                _showErrorsToggle.onToggle = action;
                _showExceptionsToggle.onToggle = action;
                _showLogsToggle.onToggle = action;
                _showWarningsToggle.onToggle = action;
            }

            /// <summary>
            /// Returns true if we are able to determine the Debugging Item's type
            /// </summary>
            /// <param name="type"></param>
            /// <returns></returns>
            public bool CanShowType(LogType type)
            {
                switch (type)
                {
                    case LogType.Assert:
                        {
                            if (!showAssertions)
                                return false;
                        }
                        break;

                    case LogType.Error:
                        {
                            if (!showErrors)
                                return false;
                        }
                        break;

                    case LogType.Exception:
                        {
                            if (!showExceptions)
                                return false;
                        }
                        break;

                    case LogType.Log:
                        {
                            if (!showLogs)
                                return false;
                        }
                        break;

                    case LogType.Warning:
                        {
                            if (!showWarnings)
                                return false;
                        }
                        break;
                }

                return true;
            }
        }

        /// <summary>
        /// Contains all elements that handle navigation in this tool
        /// </summary>
        [System.Serializable]
        public class PageNavigation
        {
            public Button backToMainFromLogButton = null;
            public GameObject errorDetailsPage = null;
            public Button exitConsoleButton = null;
            public GameObject mainPage = null;
            public GameObject optionsPage = null;
            public Button showLogButton = null;
            public Button showMainConsoleButton = null;
            public Button showOptionsButton = null;
        }

        /// <summary>
        /// Contains many of the strings in this tool
        /// </summary>
        private static class Strings
        {
            public const string debuggingToolPrefabName = "Lairinus.ProjectDebuggingTool";
            public const string Error_InitializeBeforeUse = "Error: UIDebuggingConsoleTool\nPlease initialize the tool by calling UIDebuggingConsoleTool.Initialize() before attempting to show any messages. Alternatively, the Lairinus.UIDebuggingTool prefab can be dragged in the scene.";
            public const string Error_PackageIssue = "Lairinus.Utility.DebuggingTool \n" + "Please re-download this package as soemthing went seriously wrong. In the future, please do not modify the DebuggingTool prefab...";
            public const string Error_RequireMonobehaviour = "Error: Lairinus.Utility.UIDebuggingConsoleTool\n You need to provide a valid MonoBehaviour object in order to initialize the UIDebuggingConsoleTool object. Any MonoBehaviour object can be used!";
            public const string errorDetailsPageKey = "ErrorDetailsPage";
            public const string mainPageKey = "MainPage";
            public const string optionsPageKey = "OptionsPage";
            public const string quantitySeparator = "x";
            public const string updateErrorLogRoutine = "UpdateErrorLogDetailPage_Routine";
        }
    }
}
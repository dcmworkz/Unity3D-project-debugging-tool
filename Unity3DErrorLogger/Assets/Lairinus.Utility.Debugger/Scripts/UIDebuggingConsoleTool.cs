using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using System.Collections;
using System.Linq;

namespace Lairinus.Utility
{
    public class UIDebuggingConsoleTool : MonoBehaviour
    {
        private Dictionary<string, DebuggingItem> _allDebuggingModelItems = new Dictionary<string, DebuggingItem>();

        private Dictionary<string, UIDebuggingItem> _allDebuggingViewItems = new Dictionary<string, UIDebuggingItem>();

        private string _currentlyShownPage = "";

        private DebuggingItem _currentlyViewedItem = null;

        private List<UIDebuggingItem> _displayedUIDebuggingItems = new List<UIDebuggingItem>();

        [SerializeField] private ErrorDetailPageElements _errorDetailPage = new ErrorDetailPageElements();

        private bool _isInitialized = false;

        [SerializeField] private PageNavigation _navigation = new PageNavigation();

        [SerializeField] private Options _options = new Options();

        [SerializeField] private DebuggingOptionsSO _styleSO = null;

        [SerializeField] private UIDebuggingItem _uiDebuggingItemPrefab = null;

        [SerializeField] private Transform _uiDebuggingItemTransformParent = null;

        /// <summary>
        /// Returns true if there is at least one debugging item that exists
        /// </summary>
        public bool debuggingItemsExist { get { return _allDebuggingModelItems.Values.Count > 0; } }

        private static UIDebuggingConsoleTool _instance { get; set; }

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

        private static IEnumerator AddIssueItemRoutine(string name, string stackTrace, LogType type)
        {
            _instance = GetInstanceInternal();
            yield return null;
            _instance.InitializeInternal();
            _instance.ProcessAddLogItem_Internal(name, stackTrace, type);
        }

        private static UIDebuggingConsoleTool GetInstanceInternal()
        {
            if (_instance == null)
            {
                _instance = Instantiate(Resources.Load("Lairinus.DebuggingConsole")) as UIDebuggingConsoleTool;
                return _instance;
            }
            else return _instance;
        }

        // Handles general Unity exceptions.
        private void HandleException(string name, string stackTrace, LogType type)
        {
            ProcessAddLogItem_Internal(name, stackTrace, type);
        }

        /// <summary>
        /// By this point, all items are already in the _displayed list, so we just need to show/hide the items from there.
        /// </summary>
        private void HandleOnToggle_RecalculateShownValues_Intenral()
        {
            _displayedUIDebuggingItems.ForEach(x => x.gameObject.SetActive(false));

            foreach (KeyValuePair<string, UIDebuggingItem> kvp in _allDebuggingViewItems)
            {
                _displayedUIDebuggingItems = new List<UIDebuggingItem>();
                UIDebuggingItem item = kvp.Value;
                if (item == null)
                    continue;

                // We only want to show the View object if the object is allowed to be shown.
                if (_options.CanShowType(item.type))
                    _displayedUIDebuggingItems.Add(item);
            }

            _displayedUIDebuggingItems.ForEach(x => x.gameObject.SetActive(true));
        }

        private void InitializeInternal()
        {
            if (_instance != null && _instance != this)
                Destroy(gameObject);
            _instance = this;

            try
            {
                if (!_isInitialized)
                {
                    _isInitialized = true;
                    Application.logMessageReceived -= HandleException;
                    Application.logMessageReceived += HandleException;
                    _navigation.showLogButton.onClick.AddListener(() => OnClick_ShowPage(Strings.mainPageKey));
                    _navigation.showMainConsoleButton.onClick.AddListener(() => OnClick_ShowPage(Strings.mainPageKey));
                    _navigation.showOptionsButton.onClick.AddListener(() => OnClick_ShowPage(Strings.optionsPageKey));
                    _navigation.exitConsoleButton.onClick.AddListener(() => OnClick_Close());
                    _navigation.backToMainFromLogButton.onClick.AddListener(() => OnClick_ShowPage(Strings.mainPageKey));
                    _options.AddEventListeners(() => HandleOnToggle_RecalculateShownValues_Intenral());
                }
            }
            catch
            {
                // Do nothing, because we can't do anything. The debugging tool has a user error.
                Debug.LogError(Strings.Error_PackageIssue);
            }
        }

        // Prepares, and then closes the Debugging tool
        private void OnClick_Close()
        {
            _navigation.mainPage.SetActive(false);
            _navigation.optionsPage.SetActive(false);
            _navigation.errorDetailsPage.SetActive(false);
        }

        private void OnClick_ShowDebuggingItemLogDetails(DebuggingItem item)
        {
            _currentlyViewedItem = item;
            OnClick_ShowPage(Strings.errorDetailsPageKey);
        }

        // Prepares and then shows the page
        private void OnClick_ShowPage(string pageName)
        {
            _currentlyShownPage = pageName;
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

        private void OnDisable()
        {
            Application.logMessageReceived -= HandleException;
        }

        private void OnEnable()
        {
            Application.logMessageReceived -= HandleException;
            Application.logMessageReceived += HandleException;
        }

        /// <summary>
        /// Processes adding the item after we're sure we have a Singleton to use. Internal use only.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="stackTrace"></param>
        /// <param name="type"></param>
        private void ProcessAddLogItem_Internal(string name, string stackTrace, LogType type)
        {
            // Creates the Model and prepares it for use
            string key = name + stackTrace;
            DebuggingItem item = null;
            if (_instance._allDebuggingModelItems.ContainsKey(key))
                item = _instance._allDebuggingModelItems[key];
            else
            {
                item = new DebuggingItem(name, stackTrace, type);
                _instance._allDebuggingModelItems.Add(key, item);
            }

            item.lastOccurence = Time.realtimeSinceStartup;
            item.lastOccurenceLocalTime = DateTime.Now.ToShortTimeString();
            item.count++;

            // Creates the View (if needed) and applies the model to it
            UIDebuggingItem uiDebuggingItem = null;
            if (_instance._allDebuggingViewItems.ContainsKey(key))
                uiDebuggingItem = _instance._allDebuggingViewItems[key];
            else
            {
                uiDebuggingItem = Instantiate(_instance._uiDebuggingItemPrefab, _instance._uiDebuggingItemTransformParent);
                _instance._allDebuggingViewItems.Add(key, uiDebuggingItem);
            }

            _instance._styleSO.SetDebuggingItem(uiDebuggingItem, type);
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
            if (_instance._allDebuggingModelItems.Values.Count > 0)
                _instance._navigation.showLogButton.gameObject.SetActive(true);
            else
                _instance._navigation.showLogButton.gameObject.SetActive(false);
        }

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

        // Contains everything relating to navigation in this tool
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

        private class Strings
        {
            public const string errorDetailsPageKey = "ErrorDetailsPage";
            public const string mainPageKey = "MainPage";
            public const string optionsPageKey = "OptionsPage";
            public const string Error_PackageIssue = "Lairinus.Utility.DebuggingTool \n" + "Please re-download this package as soemthing went seriously wrong. In the future, please do not modify the DebuggingTool prefab...";
            public const string quantitySeparator = "x";
            public const string updateErrorLogRoutine = "UpdateErrorLogDetailPage_Routine";
        }
    }
}
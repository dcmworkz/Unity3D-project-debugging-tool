using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using System.Collections;

namespace Lairinus.Utility
{
    public class UIDebuggingConsoleTool : MonoBehaviour
    {
        private const string errorDetailsPageKey = "ErrorDetailsPage";
        private const string mainPageKey = "MainPage";
        private const string optionsPageKey = "OptionsPage";
        private Dictionary<string, DebuggingItem> _allDebuggingItems = new Dictionary<string, DebuggingItem>();
        private Dictionary<string, UIDebuggingItem> _allUIDebuggingItems = new Dictionary<string, UIDebuggingItem>();

        private string _currentlyShownPage = "";

        private DebuggingItem _currentlyViewedItem = null;

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
        public bool debuggingItemsExist { get { return _allDebuggingItems.Values.Count > 0; } }

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
                    _navigation.showLogButton.onClick.AddListener(() => OnClick_ShowPage(mainPageKey));
                    _navigation.showMainConsoleButton.onClick.AddListener(() => OnClick_ShowPage(mainPageKey));
                    _navigation.showOptionsButton.onClick.AddListener(() => OnClick_ShowPage(optionsPageKey));
                    _navigation.exitConsoleButton.onClick.AddListener(() => OnClick_Close());
                    _navigation.backToMainFromLogButton.onClick.AddListener(() => OnClick_ShowPage(mainPageKey));
                }
            }
            catch
            {
                // Do nothing, because we can't do anything. The debugging tool has a user error.
                Debug.LogError("Lairinus.Utility.DebuggingTool \n" + "Please re-create the Debugging Console prefab in your scene. Do NOT modify this or else the tool will not work!");
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
            OnClick_ShowPage(errorDetailsPageKey);
        }

        // Prepares and then shows the page
        private void OnClick_ShowPage(string pageName)
        {
            _currentlyShownPage = pageName;
            switch (pageName)
            {
                case mainPageKey:
                    {
                        StopCoroutine("UpdateErrorLogDetailPage_Routine");
                        _navigation.mainPage.SetActive(true);
                        _navigation.optionsPage.SetActive(false);
                        _navigation.errorDetailsPage.SetActive(false);
                    }
                    break;

                case optionsPageKey:
                    {
                        StopCoroutine("UpdateErrorLogDetailPage_Routine");
                        _navigation.mainPage.SetActive(false);
                        _navigation.optionsPage.SetActive(true);
                        _navigation.errorDetailsPage.SetActive(false);
                    }
                    break;

                case errorDetailsPageKey:
                    {
                        StartCoroutine("UpdateErrorLogDetailPage_Routine");
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
            if (_instance._allDebuggingItems.ContainsKey(key))
                item = _instance._allDebuggingItems[key];
            else
            {
                item = new DebuggingItem(name, stackTrace, type);
                _instance._allDebuggingItems.Add(key, item);
            }

            item.lastOccurence = Time.realtimeSinceStartup;
            item.lastOccurenceLocalTime = DateTime.Now.ToShortTimeString();
            item.count++;

            // Creates the View (if needed) and applies the model to it
            UIDebuggingItem uiDebuggingItem = null;
            if (_instance._allUIDebuggingItems.ContainsKey(key))
                uiDebuggingItem = _instance._allUIDebuggingItems[key];
            else
            {
                uiDebuggingItem = Instantiate(_instance._uiDebuggingItemPrefab, _instance._uiDebuggingItemTransformParent);
                uiDebuggingItem.gameObject.SetActive(true);
                _instance._allUIDebuggingItems.Add(key, uiDebuggingItem);
            }

            _instance._styleSO.SetDebuggingItem(uiDebuggingItem, type);
            uiDebuggingItem.titleText.text = item.name;
            uiDebuggingItem.countText.text = "x " + item.count.ToString();
            uiDebuggingItem.backgroundButton.onClick.RemoveAllListeners();
            uiDebuggingItem.backgroundButton.onClick.AddListener(() => _instance.OnClick_ShowDebuggingItemLogDetails(item));

            if (_instance._allDebuggingItems.Values.Count > 0)
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
    }
}
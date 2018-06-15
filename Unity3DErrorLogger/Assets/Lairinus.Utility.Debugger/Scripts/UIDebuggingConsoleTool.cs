using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

public class UIDebuggingConsoleTool : MonoBehaviour
{
    public Dictionary<string, DebuggingItem> _allDebuggingItems = new Dictionary<string, DebuggingItem>();
    public Dictionary<string, UIDebuggingItem> _allUIDebuggingItems = new Dictionary<string, UIDebuggingItem>();
    private const string errorDetailsPageKey = "ErrorDetailsPage";
    private const string mainPageKey = "MainPage";
    private const string optionsPageKey = "OptionsPage";
    private string _currentlyShownPage = "";
    private DebuggingItem _currentlyViewedItem = null;
    [SerializeField] private ErrorDetailPageElements _errorDetailPage = new ErrorDetailPageElements();
    [SerializeField] private PageNavigation _navigation = new PageNavigation();
    [SerializeField] private UIDebuggingItem _uiDebuggingItemPrefab = null;
    [SerializeField] private Transform _uiDebuggingItemTransformParent = null;
    [SerializeField] private DebuggingOptionsSO debuggingOptions = null;
    public static UIDebuggingConsoleTool instance { get; private set; }

    /// <summary>
    /// Adds a Log Item to the Debugging Console
    /// </summary>
    /// <param name="name">name of the Log Item</param>
    /// <param name="stackTrace">Where the log item occurred</param>
    /// <param name="type">Which Log Type is this item, based on Unity's defaults?</param>
    public void AddLogItem(string name, string stackTrace, LogType type)
    {
        if (instance == null)
            return;

        // Creates the Model and prepares it for use
        string key = name + stackTrace;
        DebuggingItem item = null;
        if (instance._allDebuggingItems.ContainsKey(key))
            item = instance._allDebuggingItems[key];
        else
        {
            item = new DebuggingItem(name, stackTrace, type);
            instance._allDebuggingItems.Add(key, item);
        }

        item.lastOccurence = Time.realtimeSinceStartup;
        item.lastOccurenceLocalTime = DateTime.Now.ToShortTimeString();
        item.count++;

        // Creates the View (if needed) and applies the model to it
        UIDebuggingItem uiDebuggingItem = null;
        if (_allUIDebuggingItems.ContainsKey(key))
            uiDebuggingItem = _allUIDebuggingItems[key];
        else
        {
            uiDebuggingItem = Instantiate(_uiDebuggingItemPrefab, _uiDebuggingItemTransformParent);
            uiDebuggingItem.gameObject.SetActive(true);
            _allUIDebuggingItems.Add(key, uiDebuggingItem);
        }

        debuggingOptions.SetDebuggingItem(uiDebuggingItem, type);
        uiDebuggingItem.titleText.text = item.name;
        uiDebuggingItem.countText.text = item.count.ToString();
        uiDebuggingItem.backgroundButton.onClick.RemoveAllListeners();
        uiDebuggingItem.backgroundButton.onClick.AddListener(() => OnClick_ShowDebuggingItemLogDetails(item));
    }

    // Handles general Unity exceptions.
    private void HandleException(string name, string stackTrace, LogType type)
    {
        if (instance == null)
            return;

        instance.AddLogItem(name, stackTrace, type);
    }

    private void Awake()
    {
        Application.logMessageReceived -= HandleException;
        Application.logMessageReceived += HandleException;
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
                    _navigation.mainPage.SetActive(true);
                    _navigation.optionsPage.SetActive(false);
                    _navigation.errorDetailsPage.SetActive(false);
                }
                break;

            case optionsPageKey:
                {
                    _navigation.mainPage.SetActive(false);
                    _navigation.optionsPage.SetActive(true);
                    _navigation.errorDetailsPage.SetActive(false);
                }
                break;

            case errorDetailsPageKey:
                {
                    _navigation.mainPage.SetActive(false);
                    _navigation.optionsPage.SetActive(false);
                    _navigation.errorDetailsPage.SetActive(true);
                }
                break;
        }
    }

    private void Start()
    {
        // We only want a singleton
        DontDestroyOnLoad(gameObject);
        if (instance != null && instance != this)
            Destroy(gameObject);
        instance = this;

        try
        {
            _navigation.showLogButton.onClick.AddListener(() => OnClick_ShowPage(mainPageKey));
            _navigation.showMainConsoleButton.onClick.AddListener(() => OnClick_ShowPage(mainPageKey));
            _navigation.showOptionsButton.onClick.AddListener(() => OnClick_ShowPage(optionsPageKey));
            _navigation.exitConsoleButton.onClick.AddListener(() => OnClick_Close());
            _navigation.backToMainFromLogButton.onClick.AddListener(() => OnClick_ShowPage(mainPageKey));
        }
        catch
        {
            // Do nothing, because we can't do anything. The debugging tool has a user error.
            Debug.LogError("Lairinus.Utility.DebuggingTool \n" + "Please re-create the Debugging Console prefab in your scene. Do NOT modify this or else the tool will not work!");
        }
    }

    private void Update()
    {
        try
        {
            UpdateErrorDetailPage();
            AddLogItem("New log item" + UnityEngine.Random.Range(0, 50).ToString(), "No stack trace...", (LogType)UnityEngine.Random.Range(0, 3));
            string str = "sdlkfj";
            float.Parse(str);
        }
        catch (System.Exception ex)
        {
            // We don't care about catching any exceptions that occur inside here
            AddLogItem(ex.Message, ex.StackTrace, LogType.Exception);
        }
    }

    private void UpdateErrorDetailPage()
    {
        if (_currentlyViewedItem != null)
        {
            _errorDetailPage.nameText.text = _currentlyViewedItem.name;
            _errorDetailPage.countText.text = _currentlyViewedItem.count.ToString();
            _errorDetailPage.stackTraceText.text = _currentlyViewedItem.stacktrace;
            _errorDetailPage.lastOccurenceText.text = "Time since play: " + _currentlyViewedItem.lastOccurence.ToString() + "\n" + "Actual Time: " + _currentlyViewedItem.lastOccurenceLocalTime;
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
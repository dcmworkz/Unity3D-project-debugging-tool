using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class UIDebuggingConsoleTool : MonoBehaviour
{
    public static UIDebuggingConsoleTool instance { get; private set; }
    private const string errorDetailsPageKey = "ErrorDetailsPage";
    private const string mainPageKey = "MainPage";
    private const string optionsPageKey = "OptionsPage";
    [SerializeField] private PageNavigation _navigation = new PageNavigation();
    [SerializeField] private DebuggingOptionsSO debuggingOptions = null;
    public Dictionary<string, DebuggingItem> _allDebuggingItems = new Dictionary<string, DebuggingItem>();
    public Dictionary<string, UIDebuggingItem> _allUIDebuggingItems = new Dictionary<string, UIDebuggingItem>();
    [SerializeField] private UIDebuggingItem _uiDebuggingItemPrefab = null;

    // Prepares, and then closes the Debugging tool
    private void OnClick_Close()
    {
        _navigation.mainPage.SetActive(false);
        _navigation.optionsPage.SetActive(false);
    }

    // Prepares and then shows the page
    private void OnClick_ShowPage(string pageName)
    {
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

    private void Awake()
    {
        Application.logMessageReceived -= HandleException;
        Application.logMessageReceived += HandleException;
    }

    // Handles general Unity exceptions.
    private static void HandleException(string name, string stackTrace, LogType type)
    {
        if (instance == null)
            return;

        instance.AddLogItem(name, stackTrace, type);
    }

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

        item.lastOccurance = Time.deltaTime;
        item.count++;

        // Creates the View (if needed) and applies the model to it
        UIDebuggingItem uiDebuggingItem = null;         // TODO: CREATE THE VIEW OBJECT IF IT DOESN'T EXIST!!!!
        debuggingOptions.SetDebuggingItem(uiDebuggingItem, type);
        uiDebuggingItem.titleText.text = item.name;
        uiDebuggingItem.stacktraceText.text = item.stacktrace;
        uiDebuggingItem.countText.text = item.count.ToString();
        uiDebuggingItem.backgroundButton.onClick.RemoveAllListeners();
        uiDebuggingItem.backgroundButton.onClick.AddListener(() => OnClick_ShowDebuggingItemLogDetails(item));
    }

    private void OnClick_ShowDebuggingItemLogDetails(DebuggingItem item)
    {
        // Opens a page that will show all of the item's details...
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
        }
        catch
        {
            // Do nothing, because we can't do anything. The debugging tool has a user error.
            Debug.LogError("Lairinus.Utility.DebuggingTool \n" + "Please re-create the Debugging Console prefab in your scene. Do NOT modify this or else the tool will not work!");
        }
    }

    // Contains everything relating to navigation in this tool
    [System.Serializable]
    public class PageNavigation
    {
        public GameObject errorDetailsPage = null;
        public Button exitConsoleButton = null;
        public GameObject mainPage = null;
        public GameObject optionsPage = null;
        public Button showLogButton = null;
        public Button showMainConsoleButton = null;
        public Button showOptionsButton = null;
    }
}
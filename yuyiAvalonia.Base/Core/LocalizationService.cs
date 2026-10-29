using System.Globalization;
using System.Text.Json;

namespace yuyiAvalonia.Base.Core;

public class LocalizationService
{
    private static LocalizationService _instance;
    private Dictionary<string, string> _translations;
    private string _currentLanguage = CultureInfo.CurrentUICulture.Name;
    private const string ResourcesPath = "Assets/i18n/";

    public static LocalizationService Instance => _instance ??= new LocalizationService();

    public string CurrentLanguage
    {
        get => _currentLanguage;
        set
        {
            if (_currentLanguage != value)
            {
                _currentLanguage = value;
                LoadTranslations();
                LanguageChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    public event EventHandler LanguageChanged = delegate { };

    private LocalizationService()
    {
        _translations = new Dictionary<string, string>();
        LoadTranslations();
    }

    public void LoadTranslations()
    {
        var filePath = Path.Combine(AppContext.BaseDirectory, ResourcesPath, $"{_currentLanguage}.json");
        if (File.Exists(filePath))
        {
            var jsonContent = File.ReadAllText(filePath);
            _translations = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonContent) ?? new Dictionary<string, string>();
        }
        else
        {
            _translations = new Dictionary<string, string>();
        }
    }

    public string this[string key] => _translations.TryGetValue(key, out var value) ? value : key;

    public void Add(string key, string value)
    {
        _translations[key] = value;
    }

    public void NotifyLangChange()
    {
        LanguageChanged?.Invoke(this, EventArgs.Empty);
    }

    public void SetLanguage(string languageCode)
    {
        CurrentLanguage = languageCode;
        Thread.CurrentThread.CurrentCulture = new CultureInfo(languageCode);
        Thread.CurrentThread.CurrentUICulture = new CultureInfo(languageCode);
        LoadTranslations();
    }

    public IEnumerable<string> GetAvailableLanguages()
    {
        var directory = Path.Combine(AppContext.BaseDirectory, ResourcesPath);
        if (Directory.Exists(directory))
        {
            var files = Directory.GetFiles(directory, "*.json");
            return files.Select(Path.GetFileNameWithoutExtension);
        }
        return new List<string>();
    }

    
}
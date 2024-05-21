using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class UI_InitializeUser : UIBase {
    [SerializeField] private Button touchToStart;
    [SerializeField] Button completeBtn;
    [SerializeField] Text warnText;
    [SerializeField] InputField inputField;
    [SerializeField] private GameObject[] inputUserNameUis;

    [SerializeField] private int minNameLength;
    [SerializeField] private int maxNameLength;

    [SerializeField] private string warnTextEmpty;
    [SerializeField] private string warnTextInvalid;
    [SerializeField] private string warnTextTooLongShort;
    private string inputName;

    private bool isValid = false;

    private void Start() {
        Initialize();
        warnText.gameObject.SetActive(false);
    }

    private void Initialize() {
        AddCallbacks();
    }

    private void AddCallbacks() {
        inputField.onValueChanged.AddListener(UpdateName);
        completeBtn.onClick.AddListener(CompleteBtnCallback);
        touchToStart.onClick.AddListener(CheckIDExist);
    }

    private void CheckIDExist() {
        if (ES3.KeyExists("userName")) {
            isValid = true;
            touchToStart.gameObject.SetActive(false);
            inputName = DataManager.Instance.Load<string>("userName");
            SceneChange();
        }
        else {
            isValid = true;
            touchToStart.gameObject.SetActive(false);
            inputName = GenerateRandomId();
            SceneChange();
        }
    }


    public void UpdateName(string input) {
        inputName = input.Trim();
        StringBuilder warningMessage = new StringBuilder();
        if (string.IsNullOrEmpty(inputName)) {
            warningMessage.Append(warnTextEmpty).Append("\n");
        }

        if (warningMessage.Length > 0) {
            Alert(warningMessage.ToString());
            isValid = false;
        }
        else {
            warnText.gameObject.SetActive(false);
            isValid = true;
        }
    }

    private void CompleteBtnCallback() {
        if (isValid)
            SceneChange();
    }

    private void SceneChange() {
        GameManager.instance.SetNickName(inputName);
        CloseUI();
        SceneManager.LoadScene(1);
    }

    private void Alert(string message) {
        warnText.text = message;
        warnText.gameObject.SetActive(true);
    }

    string GenerateRandomId() {
        string guid = System.Guid.NewGuid().ToString();
        string cleanedGuid = guid.Replace("-", "").ToLower();

        int desiredLength = 8;
        string randomId = cleanedGuid.Substring(0, Mathf.Min(cleanedGuid.Length, desiredLength));

        return "Userid_" + randomId;
    }

    public override void ShowQuestRoot(EAchievementType type) {
       
    }
}

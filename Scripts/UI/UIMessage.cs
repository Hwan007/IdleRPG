using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIMessage : UIBase {
    #region 생략
    public TMP_Text msgText;
    public Image background;

    [Header("확인용")]
    public string msg;
    public float movingUpT;
    public float fadeOutT;
    public float speed;

    private Coroutine showing;
    private Vector3 origin;
    private Vector3 moved;
    private Color backgroundColor;
    private Color msgColor;
    #endregion
    public void ShowUI(string message, float movingUpTime, float fadeOutTime, float speed) {
        msg = message;
        movingUpT = movingUpTime;
        fadeOutT = fadeOutTime;
        this.speed = speed;

        if (showing != null) {
            StopCoroutine(showing);
            transform.position = origin;
            background.color = backgroundColor;
            msgText.color = msgColor;
        }

        showing = StartCoroutine(Message(message, movingUpTime, fadeOutTime, speed));
    }

    private IEnumerator Message(string message, float movingUpTime, float fadeOutTime, float speed) {
        origin = transform.position;
        backgroundColor = background.color;
        msgColor = msgText.color;
        msgText.text = message;

        yield return null;

        float elapsedT = .0f;

        while (elapsedT < movingUpTime) {
            elapsedT += Time.deltaTime;
            yield return null;
        }

        elapsedT = .0f;
        moved = transform.position;

        while (elapsedT < fadeOutTime) {
            elapsedT += Time.deltaTime;
            transform.position = moved + Vector3.up * (speed * elapsedT);
            background.color = backgroundColor - new Color(0, 0, 0, backgroundColor.a / fadeOutTime * elapsedT);
            msgText.color = msgColor - new Color(0, 0, 0, backgroundColor.a / fadeOutTime * elapsedT);
            yield return null;
        }

        CloseUI();
        transform.position = origin;
        background.color = backgroundColor;
        msgText.color = msgColor;
        msgText.text = "";
    }
    #region 생략
    public override void CloseUI() {
        base.CloseUI();

        gameObject.SetActive(false);
    }

    public override void ShowQuestRoot(EAchievementType type) {

    }
    #endregion
}
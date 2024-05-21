using System.Collections;
using System.Collections.Generic;
using System.Text;
using Defines;
using Keiwando.BigInteger;
using UnityEngine;
using Utils;

public class MessageUIManager : MonoBehaviour {
    public static MessageUIManager instance;
    #region 생략
    [Header("데미지 표시 관련")]
    [SerializeField] private RectTransform damageCanvas;
    [SerializeField] private UIDamage damagePrefab;
    [SerializeField] private int damagePoolSize;

    private CustomPool<UIDamage> damagePool;

    [Header("화면 중앙 메시지 표시 관련")]
    [SerializeField] private RectTransform messageCanvas;
    [SerializeField] private UIMessage messagePrefab;
    [SerializeField] private float fixedTime;
    [SerializeField] private int messagePoolSize;
    [SerializeField] private float movingUpTime;
    [SerializeField] private float fadeOutTime;
    [SerializeField] private float speed;
    private Queue<string> messageQueue;
    private CustomPool<UIMessage> messagePool;

    [Header("재화 획득 메시지 표시 관련")]
    [SerializeField] private RectTransform obtainMessageCanvas;
    [SerializeField] private UIObtainMessage obtainMessagePrefab;
    [SerializeField] private float obtainFadeOutTime;
    [SerializeField] private float obtainShowTime;
    [SerializeField] private int obtainMessagePoolSize;

    private CustomPool<UIObtainMessage> obtainMessagePool;

    private void Awake() {
        instance = this;
    }
    #endregion
    public void InitPopMessageUImanager() {
        instance = this;
        damagePool = EasyUIPooling.MakePool(damagePrefab, damageCanvas,
            x => x.actOnCallback += () => damagePool.Release(x),
            x => x.transform.SetAsLastSibling(),
            null, damagePoolSize, false);

        messagePool = EasyUIPooling.MakePool(messagePrefab, messageCanvas,
            x => x.actOnCallback += () => messagePool.Release(x),
            x => x.transform.SetAsLastSibling(),
            null, messagePoolSize, false);

        obtainMessagePool = EasyUIPooling.MakePool(obtainMessagePrefab, obtainMessageCanvas,
            x => x.actOnCallback += () => obtainMessagePool.Release(x),
            x => x.transform.SetAsLastSibling(),
            null, obtainMessagePoolSize, false);

        messageQueue = new Queue<string>();

        StartCoroutine(ShowMessage());
    }

    private IEnumerator ShowMessage() {
        float elaspedTime = .0f;
        while (true) {
            elaspedTime += Time.deltaTime;
            if (elaspedTime > fixedTime) {
                if (messageQueue.TryDequeue(out string value)) {
                    ShowCenterMessage(value);
                    elaspedTime = .0f;
                }
            }
            yield return null;
        }
    }

    public void ShowPower(BigInteger current, BigInteger diff) {
        if (diff == 0)
            return;
        StringBuilder sb = new StringBuilder();

        sb.Append("전투력 ");
        sb.Append((current).ChangeToShort());
        sb.Append(" (");
        if (diff < 0) {
            sb.Append(CustomText.SetColor($"\u25bc {BigInteger.Abs(diff).ChangeToShort()}", Color.cyan));
        }
        else {
            sb.Append(CustomText.SetColor($"\u25b2 {diff.ChangeToShort()}", Color.red));
        }
        sb.Append(")");

        ShowCenterMessage(sb.ToString());
    }

    public void ShowDamage(Vector3 position, BigInteger damage, bool isCrit = false) {
        var obj = damagePool.Get();
        obj.ShowUI(position, damage, isCrit);
    }

    public void ShowCenterMessage(string message) {
        var msg = messagePool.Get();
        msg.ShowUI(message, movingUpTime, fadeOutTime, speed);
    }

    public void ShowObtainMessage(ECurrencyType currencyType, string amount) {
        var obj = GetObtainMessage();
        obj.ShowUI(currencyType, amount, obtainShowTime, obtainFadeOutTime);
    }

    private UIObtainMessage GetObtainMessage() {
        var obj = obtainMessagePool.Get();
        if (obj.gameObject.activeInHierarchy)
            obj.transform.SetAsLastSibling();
        else
            obj.transform.SetParent(obtainMessageCanvas);
        obtainMessagePool.Release(obj);
        return obj;
    }
}
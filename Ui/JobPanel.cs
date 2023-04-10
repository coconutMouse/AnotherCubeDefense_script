using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Photon.Pun;

public struct jobSelectionButton
{
    public Button selectionButton;
    public Image IconImage;
    public Image borderImage;
    public Text tagText;
}

public class JobPanel : MonoBehaviour
{
    public JobStorage jobStorage;

    private RectTransform rectTransform;
    private Vector2 activePosition;
    private Vector2 inactivePosition;
    private PlayerCharacter myPlayerCharacter;
    private jobSelectionButton[] jobSelectionButtonGroup;
    public void ActivePanel(bool _active)
    {
        if (_active)
            rectTransform.DOAnchorPos(activePosition, 1);
        else
            rectTransform.DOAnchorPos(inactivePosition, 1);
    }

    private void Awake()
    {
        PlayerManager.Instance.OnMyPlayerCharacterActive += OnMyPlayerCharacterActive;
    }
    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        inactivePosition = rectTransform.anchoredPosition;
        activePosition = new Vector2(inactivePosition.x, -110);
        jobSelectionButtonGroup = new jobSelectionButton[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            jobSelectionButtonGroup[i] = new jobSelectionButton();
            jobSelectionButtonGroup[i].selectionButton = child.GetComponent<Button>();
            jobSelectionButtonGroup[i].IconImage = child.GetChild(0).GetComponent<Image>();
            jobSelectionButtonGroup[i].borderImage = child.GetChild(1).GetComponent<Image>();
            jobSelectionButtonGroup[i].tagText = child.GetChild(2).GetComponent<Text>();
        }
    }
    private void OnMyPlayerCharacterActive(PlayerCharacter _myPlayerCharacter)
    {
        myPlayerCharacter = _myPlayerCharacter;
        myPlayerCharacter.OnPromotionPossible += OnPromotionPossible;
    }
    private void OnPromotionPossible(string[] _promotedJobs)
    {
        SetjobSelectionButtonIcons(_promotedJobs);
        ActivePanel(true);
    }
    private void SetjobSelectionButtonIcons(string[] _promotedJobs)
    {
        int count = _promotedJobs.Length;


        for (int i = 0; i < jobSelectionButtonGroup.Length; i++)
        {
            jobSelectionButton jobSelectionButton = jobSelectionButtonGroup[i];

            if (count > 0)
            {
                string JobId = _promotedJobs[i];
                count--;
                jobSelectionButton.selectionButton.gameObject.SetActive(true);
                jobSelectionButton.selectionButton.onClick.AddListener(delegate { OnjobSelectionButtonClick(JobId); });

                JobData jobData = jobStorage.GetValue(JobId);
                jobSelectionButton.IconImage.sprite = jobData.jobIcon;
                jobSelectionButton.IconImage.color = jobData.jobColor;
                jobSelectionButton.borderImage.color = jobData.jobColor;
                jobSelectionButton.tagText.text = jobData.tag;
                jobSelectionButton.tagText.color = jobData.jobColor;
            }
            else
            {
                jobSelectionButton.selectionButton.gameObject.SetActive(false);
            }
        }
    }
    private void OnjobSelectionButtonClick(string _jobId)
    {
        foreach(jobSelectionButton jobSelectionButton in jobSelectionButtonGroup)
        {
            jobSelectionButton.selectionButton.onClick.RemoveAllListeners();
        }
        ActivePanel(false);
        myPlayerCharacter.photonView.RPC("ChangeJob", RpcTarget.All, _jobId);
    }
}

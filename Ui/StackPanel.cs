using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using Photon.Pun;

public class StackPanel : MonoBehaviour
{
    public Text pointText;
    public Text levelText;

    private bool active;
    private RectTransform rectTransform;
    private Vector2 activePosition;
    private Vector2 inactivePosition;
    private List<GameObject[]> stackObjectsGroup;
    private PlayerCharacter myPlayerCharacter;

    public void ActiveButton()
    {
        active = !active;
        if (active)
            rectTransform.DOAnchorPos(activePosition, 1);
        else
            rectTransform.DOAnchorPos(inactivePosition, 1);
    }
    public void ActiveButton(bool _active)
    {
        active = _active;
        if (active)
            rectTransform.DOAnchorPos(activePosition, 1);
        else
            rectTransform.DOAnchorPos(inactivePosition, 1);
    }
    public void OnAddStackButton(int _stackCodeNumber)
    {
        if (myPlayerCharacter.AbilityStackPoint <= 0)
            return;
        if (myPlayerCharacter == null)
        {
            Debug.LogError("myPlayerCharacter does not exist.");
            return;
        }
        myPlayerCharacter.photonView.RPC("StackIncrease", RpcTarget.All, (AbilityStackCode)_stackCodeNumber);
        int stackCount = myPlayerCharacter.GetStackCount((AbilityStackCode)_stackCodeNumber);
        SetStackObjects(_stackCodeNumber, stackCount);
    }

    private void Awake()
    {
        PlayerManager.Instance.OnMyPlayerCharacterActive += OnMyPlayerCharacterActive;
    }
    private void Start()
    {
        active = false;
        rectTransform = GetComponent<RectTransform>();
        inactivePosition = rectTransform.anchoredPosition;
        activePosition = new Vector2(120, inactivePosition.y);

        stackObjectsGroup = new List<GameObject[]>();
        Transform stackGroup = transform.GetChild(0);
        for (int i = 0; i < stackGroup.childCount; i++)
        {
            Transform stack = stackGroup.GetChild(i);
            Transform stackObjectGroup = stack.GetChild(0);
            GameObject[] stackObjects = new GameObject[stackObjectGroup.childCount];
            for (int j = 0; j < stackObjectGroup.childCount; j++)
            {
                stackObjects[j] = stackObjectGroup.GetChild(j).gameObject;
                stackObjects[j].SetActive(false);
            }
            stackObjectsGroup.Add(stackObjects);
        }
        int a = 0;
        for (int i = 0; i < stackObjectsGroup.Count; i++)
        {
            for (int j = 0; j < stackObjectsGroup[i].Length; j++)
            {
                a++;
            }
        }
    }
    private void OnMyPlayerCharacterActive(PlayerCharacter _myPlayerCharacter)
    {
        myPlayerCharacter = _myPlayerCharacter;
        myPlayerCharacter.OnLevelChange += OnLevelChange;
        myPlayerCharacter.OnAbilityStackPointChange += OnAbilityStackPointChange;
        OnLevelChange(myPlayerCharacter.Level);
        OnAbilityStackPointChange(myPlayerCharacter.AbilityStackPoint);
    }
    private void OnLevelChange(int _level)
    {
        levelText.text = _level.ToString();
        ActiveButton(true);
    }
    private void OnAbilityStackPointChange(int _stackPoint)
    {
        pointText.text = _stackPoint.ToString();
        if(_stackPoint == 0)
            ActiveButton(false);
    }

    private void SetStackObjects(int _stackCodeNumber, int _count)
    {
        foreach (GameObject stackObject in stackObjectsGroup[_stackCodeNumber])
        {
            if(_count > 0)
            {
                _count--;
                stackObject.gameObject.SetActive(true);
            }
            else
            {
                stackObject.gameObject.SetActive(false);
            }
        }
    }

}

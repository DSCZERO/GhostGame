using System.Collections;
using UnityEngine;

public class DoorController : MonoBehaviour
{
    [Header("门设置")]
    public float interactDistance = 2.5f; // 玩家与门的交互距离
    public float moveDistance = 4.0f; // 门向上移动的距离
    public float moveDuration = 2.0f; // 门移动所需时间
    
    [Header("激活要求")]
    [Tooltip("激活此门所需的按钮数量")]
    public int requiredButtonsCount = 1;
    
    // 状态
    private int activatedButtonsCount = 0;
    private bool isDoorOpen = false;
    private Vector3 initialPosition;
    private Vector3 targetPosition;
    private GhostMode playerGhostMode;
    
    void Start()
    {
        // 保存初始位置
        initialPosition = transform.position;
        targetPosition = initialPosition + Vector3.up * moveDistance;
        
        // 查找玩家的GhostMode组件
        if (Camera.main != null)
            playerGhostMode = Camera.main.GetComponentInParent<GhostMode>();
        
        if (playerGhostMode == null)
            Debug.LogWarning("找不到GhostMode组件，门功能可能受限");
    }
    
    void Update()
    {
        // 检查玩家是否在门附近，且按下E键，且能打开门，且不在灵魂模式
        if (!isDoorOpen && 
            Input.GetKeyDown(KeyCode.E) && 
            IsPlayerNearby() && 
            IsPlayerHuman() && 
            CanOpenDoor())
        {
            OpenDoor();
        }
    }
    
    private bool IsPlayerNearby()
    {
        if (Camera.main == null) return false;
        
        // 获取玩家位置 (从摄像机的父物体)
        Transform playerTransform = Camera.main.transform.parent;
        if (playerTransform == null) return false;
        
        // 计算距离
        float distance = Vector3.Distance(transform.position, playerTransform.position);
        return distance <= interactDistance;
    }
    
    private bool IsPlayerHuman()
    {
        // 如果没有找到GhostMode组件，默认允许交互
        if (playerGhostMode == null) return true;
        
        // 只有在非灵魂模式下才能交互
        return !playerGhostMode.IsInGhostMode;
    }
    
    public void RegisterButtonActivation()
    {
        activatedButtonsCount++;
        Debug.Log($"按钮已激活 ({activatedButtonsCount}/{requiredButtonsCount})");
    }
    
    private bool CanOpenDoor()
    {
        return activatedButtonsCount >= requiredButtonsCount;
    }
    
    private void OpenDoor()
    {
        if (isDoorOpen) return;
        
        isDoorOpen = true;
        StartCoroutine(MoveDoorRoutine());
    }
    
    private IEnumerator MoveDoorRoutine()
    {
        float elapsed = 0;
        
        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / moveDuration;
            
            // 平滑移动门
            transform.position = Vector3.Lerp(initialPosition, targetPosition, t);
            yield return null;
        }
        
        // 确保门到达最终位置
        transform.position = targetPosition;
    }
}
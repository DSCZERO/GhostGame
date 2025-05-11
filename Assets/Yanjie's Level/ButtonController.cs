using System.Collections;
using UnityEngine;

public class ButtonController : MonoBehaviour
{
    [Header("按钮设置")]
    public float interactDistance = 2.5f; // 玩家与按钮的交互距离
    public float colorChangeSpeed = 2.0f; // 颜色变化速度
    
    [Header("引用")]
    [Tooltip("需要由此按钮激活的门")]
    public DoorController targetDoor;
    
    // 颜色设置
    private Color deactivatedColor = Color.red;
    private Color activatedColor = Color.green;
    
    // 状态
    private bool isActivated = false;
    private Renderer buttonRenderer;
    private Material buttonMaterial;
    private GhostMode playerGhostMode;
    
    void Start()
    {
        // 获取渲染器和材质
        buttonRenderer = GetComponent<Renderer>();
        if (buttonRenderer == null)
        {
            Debug.LogError("按钮缺少渲染器组件!");
            return;
        }
        
        // 保存材质引用并设置初始颜色
        buttonMaterial = buttonRenderer.material;
        buttonMaterial.color = deactivatedColor;
        
        // 查找玩家的GhostMode组件
        if (Camera.main != null)
            playerGhostMode = Camera.main.GetComponentInParent<GhostMode>();
        
        if (playerGhostMode == null)
            Debug.LogWarning("找不到GhostMode组件，按钮功能可能受限");
    }
    
    void Update()
    {
        // 检查玩家是否在按钮附近，且按下E键，且不在灵魂模式
        if (!isActivated && 
            Input.GetKeyDown(KeyCode.E) && 
            IsPlayerNearby() && 
            IsPlayerHuman())
        {
            ActivateButton();
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
    
    private void ActivateButton()
    {
        isActivated = true;
        
        // 开始颜色渐变
        StartCoroutine(ChangeColorRoutine());
        
        // 通知目标门此按钮已激活
        if (targetDoor != null)
            targetDoor.RegisterButtonActivation();
    }
    
    private IEnumerator ChangeColorRoutine()
    {
        float t = 0;
        
        while (t < 1)
        {
            t += Time.deltaTime * colorChangeSpeed;
            buttonMaterial.color = Color.Lerp(deactivatedColor, activatedColor, t);
            yield return null;
        }
        
        // 确保最终颜色是正确的
        buttonMaterial.color = activatedColor;
    }
}
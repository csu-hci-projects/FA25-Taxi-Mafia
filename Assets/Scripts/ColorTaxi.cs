using UnityEngine;

public class ColorTaxi : MonoBehaviour
{
    [Header("References")]
    public Renderer carRenderer;

    [Header("Materials")]
    public Material redMaterial;
    public Material blueMaterial;
    public Material yellowMaterial;

    [Header("Costs")]
    public int redCost = 100;
    public int blueCost = 150;
    public int yellowCost = 200;

    [Header("Money System")]
    public MissionLogic missionLogic;  // <-- reference your existing money script

    private Material _currentMaterial;

    void Start()
    {
        _currentMaterial = carRenderer.material;
    }

    public void ChangeToRed()
    {
        TryChangeColor(redMaterial, redCost);
    }

    public void ChangeToBlue()
    {
        TryChangeColor(blueMaterial, blueCost);
    }

    public void ChangeToYellow()
    {
        TryChangeColor(yellowMaterial, yellowCost);
    }

    private void TryChangeColor(Material newMaterial, int cost)
    {
        // Already this color?
        if (_currentMaterial == newMaterial)
            return;

        // Check money
        int money = missionLogic.GetMoney();
        if (money < cost)
        {
            Debug.Log("Not enough money to change color.");
            return;
        }

        // Deduct money
        missionLogic.SetMoney(money - cost);

        // Apply material
        carRenderer.material = newMaterial;
        _currentMaterial = newMaterial;

        Debug.Log("Color changed. Remaining money: " + missionLogic.GetMoney());
    }
}

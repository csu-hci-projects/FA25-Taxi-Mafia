using UnityEngine;

public class ColorTaxi : MonoBehaviour
{
    [Header("References")]
    public Renderer carRenderer;

    [Header("Materials")]
    public Material redMaterial;
    public Material blueMaterial;
    public Material yellowMaterial;
    public Material copMaterial;
    public Material pinkMaterial;
    public Material brownMaterial;
    public Material greenMaterial;

    [Header("Costs")]
    public int redCost = 100;
    public int blueCost = 200;
    public int yellowCost = 50;
    public int copCost = 300;
    public int pinkCost = 200;
    public int brownCost = 200;
    public int greenCost = 200;

    [Header("Money System")]
    public MissionLogic missionLogic;  // <-- reference your existing money script

    private Material _currentMaterial;
    public AudioSource notEnoughMoney;

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

    public void ChangeToCop()
    {
        TryChangeColor(copMaterial, copCost);
    }
    public void ChangeToPink()
    {
        TryChangeColor(pinkMaterial, pinkCost);
    }
    public void ChangeToBrown()
    {
        TryChangeColor(brownMaterial, brownCost);
    }
    public void ChangeToGreen()
    {
        TryChangeColor(greenMaterial, greenCost);
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
            notEnoughMoney.Play();
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

using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpeedometerCtrl : MonoBehaviour
{
    [SerializeField] Rigidbody target;
    [SerializeField] float maxSpeed = 150;
    [SerializeField] float minNeedleAngle = 130f;
    [SerializeField] float maxNeedleAngle = -130f;
    [SerializeField] Transform needlePivot;
    [SerializeField] GameObject speedLabelTemplate;
    [SerializeField] int speedLabelAmount = 15;

    float GetSpeedRotation(float speed, float maxSpeed)
    {
        float totalAngleSize = minNeedleAngle - maxNeedleAngle; // Calculates the total angle the needle can turn

        float speedNormalized = speed / maxSpeed; //Calculates a percentage to apply to the angle by dividing the max speed from the current speed, i.e Current speed = 15mph / max speed = 150mph = 10%

        return speed > maxSpeed ? maxNeedleAngle : minNeedleAngle - speedNormalized * totalAngleSize; //Outputs the method's angle result, if the current speed is over the max speed it clamps the output to the max angle to prevent the angle over shooting
    }

    void Update()
    {
        needlePivot.eulerAngles = new Vector3(0, 0, GetSpeedRotation(target.velocity.magnitude * 2.23693629f, maxSpeed)); //Rotates the needle using the method we created above, reading from our target rigidbody and multiplying it to convert it to a miles per hour measure
    }

    void CreateSpeedLabels(float maxSpeed)
    {
        float totalAngleSize = minNeedleAngle - maxNeedleAngle;

        for (int i = 0; i <= speedLabelAmount; i++)
        {
            GameObject speedLabel = Instantiate(speedLabelTemplate, transform);
            float labelSpeedNormalized = (float)i / speedLabelAmount;
            float speedLabelAngle = minNeedleAngle - labelSpeedNormalized * totalAngleSize;
            speedLabel.transform.eulerAngles = new Vector3(0, 0, speedLabelAngle);
            speedLabel.GetComponentInChildren<TextMeshProUGUI>().text = (labelSpeedNormalized * maxSpeed).ToString("0");
            speedLabel.GetComponentInChildren<TextMeshProUGUI>().transform.eulerAngles = Vector3.zero;
            speedLabel.SetActive(true);
        }

        needlePivot.SetAsLastSibling();
    }

    private void Awake()
    {
        CreateSpeedLabels(maxSpeed); //Calls the method above we created providing the maxSpeed variable as the argument
        speedLabelTemplate.SetActive(false); //Sets the speed label template inactive to stop it displaying on our speedometer
    }
}

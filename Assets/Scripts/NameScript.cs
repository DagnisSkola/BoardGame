using UnityEngine;
using TMPro;
using static Unity.Burst.Intrinsics.X86.Avx;

public class NameScript : MonoBehaviour
{
    TextMeshPro tMP;

    private void Awake()
    {
        tMP = transform.Find("NameField").gameObject.GetComponent<TextMeshPro>();
    }
    
    public void SetName(string name)
    {
        tMP.text = name;
        tMP.color = new Color32((byte)Random.Range(0, 255), (byte)Random.Range(0, 255), (byte)Random.Range(0, 255),255);
    }

    public string GetDisplayName()
    {
        if (tMP != null)
            return tMP.text;
        return "Unknown";
    }

}

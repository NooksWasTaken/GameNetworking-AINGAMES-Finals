using UnityEngine;

public class MultiModelRandomizer : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] GameObject[] models = new GameObject[0];
    private bool keepCurrentModel = false;
    void Start()
    {
        if (models.Length == 0 || keepCurrentModel)
            return;

        for (int i = 0; i < models.Length; i++)
        {
            if (models[i].activeSelf)
                models[i].SetActive(false);
        }

        ActivateModelAt(Random.Range(0, models.Length));
    }

    public void ActivateModelAt(int modelIndex)
    {
        models[modelIndex].SetActive(true);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}

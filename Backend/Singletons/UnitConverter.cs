using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitConverter : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public static UnitConverter Instance;
    private void Awake() {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject.transform.root); }
        else  Destroy(gameObject); }
}

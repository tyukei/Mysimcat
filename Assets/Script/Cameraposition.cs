using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cameraposition : MonoBehaviour
{
    public GameObject targetobject;
    // Start is called before the first frame update
    void Start()
    {
    }
    // Update is called once per frame
    void Update()
    {
        transform.rotation = targetobject.transform.rotation ;
        transform.position = targetobject.transform.position;
        var rot = transform.rotation.eulerAngles;
        rot.y -= 45.0f;
        transform.rotation = Quaternion.Euler(rot);
        Vector3 pos = transform.position;
        pos.y = 1;
        transform.position = pos;
        //https://bluebirdofoz.hatenablog.com/entry/2017/08/25/225903
        //https://www.hanachiru-blog.com/entry/2019/03/08/233640
    }
}
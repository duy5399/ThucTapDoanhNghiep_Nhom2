using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Pun.UtilityScripts;

public class KickScript : MonoBehaviourPun
{
    public float destroyTime = 15f;
    // Start is called before the first frame update
    void Start()
    {
        Destroy(gameObject, destroyTime * Time.deltaTime);    //bất cứ khi nào đạn được khởi tạo nó sẽ chờ 2 giây sau đó hủy các cú Kick
    }

    // Update is called once per frame
    void Update()
    {
        //transform.Translate(Vector2.up * speed * Time.deltaTime); //Translation là sự di chuyển object trong trục tọa độ X,Y hoặc Z
        //hàm trên giúp viên đạn bay đi liên tục, bỏ đi thì đạn chỉ xuất hiện hình ảnh ở đầu súng
    }

    [PunRPC]
    public void destroy()
    {
        Destroy(this.gameObject);
    }

    [PunRPC]
    public void changeDirection()   //thay đổi phương hướng
    {
        Vector3 scale = transform.localScale;
        scale.x = -1.5f;
        transform.localScale = scale;
    }
}

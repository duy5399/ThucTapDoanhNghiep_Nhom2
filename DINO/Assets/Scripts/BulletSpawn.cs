using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Pun.UtilityScripts;

public class BulletSpawn : MonoBehaviourPun
{
    public float speed = 20f;
    public float destroyTime = 40f;

    // Start is called before the first frame update
    void Start()
    {
        Destroy(gameObject, destroyTime*Time.deltaTime);    //bất cứ khi nào đạn được khởi tạo nó sẽ chờ 2 giây sau đó hủy các Bullet
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector2.up * speed * Time.deltaTime); //Translation là sự di chuyển object trong trục tọa độ X,Y hoặc Z
        //hàm trên giúp viên đạn bay đi liên tục, bỏ đi thì đạn chỉ xuất hiện hình ảnh ở đầu súng
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        //nếu đối tượng va chạm là vật thể không phải người chơi thì tự phá hủy đạn khi va chạm
        if(    collision.gameObject.name == "box_1" || collision.gameObject.name == "box_2"
            || collision.gameObject.name == "box_3" || collision.gameObject.name == "box_4"
            || collision.gameObject.name == "box_5" || collision.gameObject.name == "box_6")
        {
            Destroy(gameObject);
        }
        ////đối tượng là người chơi
        //Destroy(gameObject);
        //collision.gameObject.GetComponent<PhotonView>().RPC("TakeDamage", RpcTarget.AllBuffered);   //gọi hàm TakeDamage (trừ máu) cho đối tượng va chạm là người chơi
        //if(/*collision.gameObject.GetComponent<MyPlayer>().currentHealth <= 0 ||*/ collision.gameObject.GetComponent<MyPlayer>().blood.fillAmount == 0f) //kiểm tra máu của người chơi mà đạn va chạm
        //{
        //    Add_Score();    //thêm điểm
        //}
    }
    //public void Add_Score()
    //{
    //    photonView.Owner.AddScore(1);   //chủ sở hữu của viên đạn được cộng điểm
    //}
}


using UnityEngine;

using NativeWebSocket;
using System.Collections;
using System.Collections.Generic;
using System;
using JetBrains.Annotations;
using System.Net;
using UnityEngine.UI;


#region Açıklama
/*
 Nurullah Çelik tarafından 28.07.2020 tarihinde yaratılmıştır.
WebSocket sistemini yönetebilmek için gerekli fonksiyonları içerir.*/

#endregion

[Serializable]
class socketauth
{
    public string cmd = "auth";
    public string token = "";
}
[Serializable]
public class Users
{
    public string username;
    public string avatar;
}
[Serializable]
public class UserValues
{
    public bool status = false;
    public Users[] users;
}

public class GameWebsocketServer : MonoBehaviour
{
    #region Değişkenler
    /// <summary>
    /// Web socket için bağlantı urlsi
    /// </summary>
    string websocketserverurl = "ws://167.71.14.193:8080/";

    //Token için gerekli sınıf.
    socketauth sauth;


    //Websocket için gerekli değişken
    WebSocket websocket;

    //Gelen user verilerinin tutulacağı UserValues sınıfının değişkeni
    UserValues tempusers;

    [Tooltip("İndirme yöneticisi scripti")]
    public DownloadManager dm;
    
    [Tooltip("Ekranda gösterilen kullanıcı verilerinin prefab nesnesi")]
    public GameObject userPrefab;
  
    //Userları listeye atmak yerine dictionary içerisine atıldı ve fazladan üretilecek nesnelerin önüne geçilmiş olundu
    public Dictionary<string, Users> ulist = new Dictionary<string, Users>();
    
    //Ekranda gösterimi yapılacak prefabların parent objesi
    public GameObject userprefabparent;
    
    //Resimleri indirebilmek için gerekli bir sınıf ve onun değişkeni
    WebClient wb = new WebClient();
    
    //Json stringi her seferinde yenileniyor bu sayede sürekli üretimin önüne geçiliyor.
    string json;

    //İnformation system scripti için bir değişken
    public InfoSystem infoSys;
    #endregion
    public void TryAuthWebSocket(string token)
    {
        sauth.token = token;
        json = JsonUtility.ToJson(sauth);

      //  Debug.Log(json);
        SendWebSocketMessage(json);
      
    }


    /// <summary>
   /// Başlangıçta websockete bağlanılır.
   /// Aslında isteğe bağlıdır. Eğer istenilirse fonksiyon ismi değiştirilerek istenildiği zaman websockete bağlanması sağlanabilir.
   /// socket auth ile auth için gerekli sınıf türetilir
   /// websocket ile websocket bağlantısı için gerekli sınıf türetilir.
   /// </summary>
    async void Start()
    {
        sauth = new socketauth();
        websocket = new WebSocket(websocketserverurl);

        //web socket açılınca tetiklenen handler
        websocket.OnOpen += () =>
        {
            infoSys.publishInfo("Websockete bağlantı kuruldu", Color.green);
        };
        //Web socket hata verince tetiklenen handler
        websocket.OnError += (e) =>
        {
            infoSys.publishInfo("Websocket hatası:\n" + e,Color.red);
        };

        //Web ocket kapatılınca tetiklenen handler
        websocket.OnClose += (e) =>
        {
            infoSys.publishInfo("Websocket kapatıldı", Color.yellow);
        };
        //Websocket tarafından mesaj gelirse tetiklenen handler
        websocket.OnMessage += (bytes) =>
        {
            //Mesajları string tipinde alınır.
            var message = System.Text.Encoding.UTF8.GetString(bytes);
            //Welcome mesajı gelirse access token ile giriş yapılabilmiştir.
            if(message.ToString().Contains("Welcome"))
            {
                infoSys.publishInfo("Websocket authentication tamamlandı", Color.green);
                dm.startstopb.enabled = true;
            }
            else
            {
                //Gelen veri parçalanması için çağırılır.
                ParseJsonUsers(message);
                
            }
             
        };

      

        // websockete bağlanılır ve mesaj beklenir.
        await websocket.Connect();
    }

    void Update()
    {
#if !UNITY_WEBGL || UNITY_EDITOR
        websocket.DispatchMessageQueue();
#endif
    }


    /// <summary>
    /// WebSockete string olarak gelen json verisi aktarılır.
    /// async olmasının sebebi takılmalara uğramadan asenkron olarak ilerletebilmektir.
    /// </summary>
    /// <param name="json"></param>
    public async void SendWebSocketMessage(string json)
    {
        if (websocket.State == WebSocketState.Open)
        {
            //Json tipinde veri gönderilir.
            await websocket.SendText(json);
        }
    }


    /// <summary>
    /// Uygulama kapatılınca websocket kapatılır.
    /// </summary>
    private async void OnApplicationQuit()
    {
        //Websocket kapatılır
        await websocket.Close();

       //Bütün child objeler temizlenir.
       for(int i =  0; i < userprefabparent.transform.childCount; i++)
        {
            if(userprefabparent.transform.GetChild(0) != null)
            {
                DestroyImmediate(userprefabparent.transform.GetChild(0).gameObject);
            }
        }
    }


    /// <summary>
    /// Value olarak gelen string değerini parçalar ve tempuser ismindeki UsersValues isimli sınıfın yeni bir değişkenini üretir.
    /// Daha sonra AddUserToUI fonksiyonunu çağırarak ekranda göstermesini sağlar
    /// 
    /// </summary>
    /// <param name="value"></param>
    private void ParseJsonUsers(string value)
    {
        tempusers = JsonUtility.FromJson<UserValues>(value);
        //Debug.Log(tempusers.status.ToString());
        AddUserToUI();
    }


    /// <summary>
    /// Parçalanmış json dosyası içerisinden çıkan kullanıcıları ekrana bastırmak için kullanılır
    /// async olmasının sebebi avatar fotoğrafları takılmadan indirebilmektir.
    /// </summary>
    private  async void AddUserToUI()
    {
        foreach(Users u in tempusers.users)
        {
            if (!ulist.ContainsKey(u.username))
            {
                try
                {
                    ulist.Add(u.username, u);
                    if (Application.isPlaying)
                    {
                        var obj = Instantiate(userPrefab, userprefabparent.transform);
                        obj.name = u.username;
                        byte[] avatar = await wb.DownloadDataTaskAsync(u.avatar);
                        Texture2D avTex = new Texture2D(1, 1);
                        avTex.LoadImage(avatar);

                        obj.transform.GetChild(1).GetComponent<Image>().sprite = Sprite.Create(avTex, new Rect(0, 0, avTex.width, avTex.height), new Vector2(0, 0));
                        obj.transform.GetChild(0).GetComponent<Text>().text = u.username;
                    }
                }
                catch(Exception e)
                {
                    Destroy(GameObject.Find(u.username));
                }
            }
        }
    }

    /// <summary>
    /// Kullanıcıları getiren komutu websocket aracılığı ile gönderir.
    /// </summary>
    public void GetUsers()
    {
        sauth.token = null;
        sauth.cmd = "getusers";

        json = JsonUtility.ToJson(sauth);
        SendWebSocketMessage(json);

    }
    
}

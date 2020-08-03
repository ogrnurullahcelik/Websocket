using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[Serializable]
class RegisterValueUser
{
    public bool status = false;
    public string token = "";
}
public class ApiServer : MonoBehaviour
{
    #region Açıklama
    /*
     Nurullah Çelik tarafından 28.08.2020 tarihinde oluşturulmuştur.
     Amacı Api Server ismi verilen işlemlerin yürütülmesini sağlamaktır.
     Post ile farklı fonksiyonlar çağırılır ve cevaplar kullanıcıya bilgilendirme yolu ile yönlendirilir.

     */
    #endregion




    #region Değişkenler


    /// <summary>
    /// Api Server url'si post'lar bu linke ekleme yaparak kullanılır.
    /// </summary>
     string apiurl = "http://167.71.14.193:8081/";


    /// <summary>
    /// Kullanıcıların giriş bilgilerini tutabilmek için kullanılan bir sınıf değişkeni.
    /// </summary>
     RegisterValueUser rvu;

     private string userName;
     private string usrPass;
     private GameWebsocketServer gws;

    public InfoSystem infoSys;
    #endregion


    // Start is called before the first frame update
    void Start()
    {
        this.gws = this.GetComponent<GameWebsocketServer>();

        //StartCoroutine(AuthUser("Nc1", "123456789"));
    }
    #region Api Server işlemleri
    public void TryRegistering()
    {
        if(!userName.Equals("") && !userName.Contains("delete") && !userName.Contains("update")
            && !usrPass.Equals("") && !usrPass.Contains("delete") && !usrPass.Contains("update"))
        {
            StartCoroutine(RegisterUser(userName, usrPass));
        }


    }
    public void TryAuthUsr()
    {
        if (!userName.Equals("") && !userName.Contains("delete") && !userName.Contains("update")
            && !usrPass.Equals("") && !usrPass.Contains("delete") && !usrPass.Contains("update"))
        {
            StartCoroutine(AuthUser(userName, usrPass));
        }


    }
    IEnumerator RegisterUser(string userName, string usrPass)
    {
        WWWForm form = new WWWForm();
        form.AddField("username", userName);
        form.AddField("password", usrPass);

        UnityWebRequest uwr = UnityWebRequest.Post(apiurl+"register", form);
        yield return uwr.SendWebRequest();

        if (uwr.isNetworkError)
        {
            infoSys.publishInfo("Registering verisi gönderilemedi", Color.red);
        }
        else
        {
          
            rvu = JsonUtility.FromJson<RegisterValueUser>(uwr.downloadHandler.text);
            if(rvu.status == true)
            {
                infoSys.publishInfo("Registering başarılı", Color.green);
            }
            else
            {
                infoSys.publishInfo("Registering başarısız", Color.red);
            }

        }
    }

    IEnumerator AuthUser(string userName, string usrPass)
    {
        WWWForm form = new WWWForm();
        form.AddField("username", userName);
        form.AddField("password", usrPass);

        UnityWebRequest uwr = UnityWebRequest.Post(apiurl + "auth", form);
        yield return uwr.SendWebRequest();

        if (uwr.isNetworkError)
        {
            //Debug.Log("Error While Sending: " + uwr.error);
            infoSys.publishInfo("Authentication verisi gönderilemedi", Color.red);
        }
        else
        {
           // Debug.Log("Received: " + uwr.downloadHandler.text);

            rvu = JsonUtility.FromJson<RegisterValueUser>(uwr.downloadHandler.text);
            if(rvu.status == true)
            {
                infoSys.publishInfo("Authentication başarılı", Color.green);
            }
            else
            {
                infoSys.publishInfo("Authentication başarısız", Color.red);
            }
        }
    }
    #endregion

    #region UI İşlemleri

    public void GetUserName(string name)
    {
        this.userName = name;
    }
    public void GetUserPass(string pass)
    {
        this.usrPass = pass;
    }
    #endregion

    #region Websocket işlemleri

    public void TryAuthWebSocket()
    {
        if(!rvu.token.Equals(""))
        gws.TryAuthWebSocket(rvu.token);
    }
    #endregion
}

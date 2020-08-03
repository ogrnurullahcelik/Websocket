using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DownloadManager : MonoBehaviour
{

    #region Butonun en son hangi state de kaldığını öğrenmek için yapı
    enum DownloadManagerButton
    {
        start,
        stop
    }

    DownloadManagerButton dmb;
    #endregion
    #region Değişkenler

    //başlat durdur butonu
    public Button startstopb;

    //Gamewebsocketserver scriptine ulaşabilmek için değişken
    GameWebsocketServer gws;

    //İndirme işlemini doğrulayan bool değişkeni
    bool download = false;

    //İndirmeyi yöneten corotuine değişkeni
    Coroutine downloadHandler;

    [Tooltip("Bilgilendirme sistemi")]
    public InfoSystem infoSystem;
    #endregion


    /// <summary>
    /// İndirme işlemini yöneten scriptin start fonksiyonu 
    /// start butonu en başta disable edilir indirmeye bağlantı kurulmadan başlamasın diye 
    /// Gamewebsocket server ise tag ile bulunarak eşitlenir.
    /// </summary>
    private void Start()
    {
        startstopb = this.GetComponent<Button>();
        startstopb.enabled = false;
        dmb = DownloadManagerButton.stop;
        gws =  GameObject.FindGameObjectWithTag("Manager").GetComponent<GameWebsocketServer>();
    }

    /// <summary>
    /// Eğer indirme varsa durduru
    /// </summary>
    public void StartStop()
    {
        if(dmb == DownloadManagerButton.start)
        {
            //Durdurulacak
            dmb = DownloadManagerButton.stop;
            startstopb.GetComponentInChildren<Text>().text = "Start";
            StopCoroutine(downloadHandler);
            infoSystem.publishInfo("İndirme durduruldu", Color.red);

           
            download = false;
        }
        else
        {
            //Başlatılacak
            dmb = DownloadManagerButton.start;
            startstopb.GetComponentInChildren<Text>().text = "Stop";
            download = true;
            downloadHandler = StartCoroutine(Trigger2Download());
            infoSystem.publishInfo("İndirme başlatıldı", Color.green);

        }
    }

    IEnumerator Trigger2Download()
    {
        gws.GetUsers();
        int waitseconds = 0;
        while (download)
        {
            yield return new WaitForSecondsRealtime(1);
            if (waitseconds == 10)
            {
                waitseconds = 0;
                gws.GetUsers();
            }
            else
                waitseconds++;
        }
    }
    private void OnApplicationQuit()
    {
        StopAllCoroutines();
    }
}

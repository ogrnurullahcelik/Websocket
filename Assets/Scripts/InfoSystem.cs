
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class InfoSystem : MonoBehaviour
{
    Animator infoAnimator;
    // Start is called before the first frame update
    private void Start()
    {
        this.infoAnimator = this.GetComponent<Animator>();
    }
    public void publishInfo(string text, Color degree)
    {

       
      
        this.GetComponentInChildren<Text>().text = text;
        this.GetComponent<Image>().color = degree;
        infoAnimator.SetBool("play", !infoAnimator.GetBool("play"));
        //StartCoroutine(publish(text,degree));

    }
    IEnumerator publish(string key, Color degree)
    {
       
        yield return new WaitForEndOfFrame();
       

    }
}

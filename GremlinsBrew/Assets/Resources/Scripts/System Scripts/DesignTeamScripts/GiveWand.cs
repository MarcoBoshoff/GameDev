using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GiveWand : MonoBehaviour
{

    public GameObject wandModel;
    public GameObject stirModel;
    public Animator anim;
    
    //void Update()
    //{
    //    CastingSpell();
    //    StirCauldron();
    //}

    public void CastingSpell(bool b) {
        anim.SetBool("IsCasting", b);
        wandModel.gameObject.SetActive(b);

    }

    public void StirCauldron(bool b) {
        anim.SetBool("IsStirring", b);
        stirModel.gameObject.SetActive(b);
    }

}

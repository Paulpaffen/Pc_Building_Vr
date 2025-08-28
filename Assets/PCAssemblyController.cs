using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PCAssemblyController : MonoBehaviour
{
    [Header("Referencia a la pieza animada")]
    public GameObject animatedPart; // la que tiene animación
    [Header("Referencia a la pieza final")]
    public GameObject finalPart;    // la duplicada estática

    private Animator animator;

    void Start()
    {
        // La pieza animada y final empiezan ocultas
        if (animatedPart != null) animatedPart.SetActive(false);
        if (finalPart != null) finalPart.SetActive(false);

        if (animatedPart != null)
            animator = animatedPart.GetComponent<Animator>();
    }

    // Se llama cuando le toca a esta pieza animarse
    public void PlayAssembly()
    {
        if (animatedPart != null)
        {
            animatedPart.SetActive(true);
            animator.Play("Assemble", -1, 0f); // nombre de la animación de ensamblaje
        }
    }

    // Se llama cuando termina la animación y debe quedar fija
    public void FinishAssembly()
    {
        if (animatedPart != null) animatedPart.SetActive(false);
        if (finalPart != null) finalPart.SetActive(true);
    }
}

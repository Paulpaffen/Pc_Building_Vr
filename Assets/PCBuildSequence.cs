using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BuildStep
{
    public string stepName;
    public GameObject animatedPart; // La pieza con la animación
    public GameObject finalPart;    // La pieza final estática
    public string animationTrigger = "Assemble"; // Nombre del trigger de animación
}

public class PCBuildSequence : MonoBehaviour
{
    public List<BuildStep> steps = new List<BuildStep>();
    private int currentStep = -1;

    public void ResetSequence()
    {
        foreach (var step in steps)
        {
            if (step.animatedPart != null) step.animatedPart.SetActive(false);
            if (step.finalPart != null) step.finalPart.SetActive(false);
        }
        currentStep = -1;
    }

    public void NextStep()
    {
        // Si ya había un paso activo → ocultar animada y mostrar final
        if (currentStep >= 0 && currentStep < steps.Count)
        {
            var prevStep = steps[currentStep];
            if (prevStep.animatedPart != null) prevStep.animatedPart.SetActive(false);
            if (prevStep.finalPart != null) prevStep.finalPart.SetActive(true);
        }

        // Pasar al siguiente paso
        currentStep++;

        if (currentStep < steps.Count)
        {
            var step = steps[currentStep];

            if (step.animatedPart != null)
            {
                step.animatedPart.SetActive(true);

                // Lanzar animación (si tiene Animator)
                Animator anim = step.animatedPart.GetComponent<Animator>();
                if (anim != null && !string.IsNullOrEmpty(step.animationTrigger))
                {
                    anim.ResetTrigger(step.animationTrigger);
                    anim.SetTrigger(step.animationTrigger);
                }
            }
        }
        else
        {
            Debug.Log("✅ Ensamblaje completo.");
        }
    }
}

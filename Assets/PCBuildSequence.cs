using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BuildStep
{
    public string stepName;

    // Prefabs
    public GameObject prefabAnimated; // Prefab con animación
    public GameObject prefabFinal;    // Prefab estático final

    // Instancias en runtime
    [HideInInspector] public GameObject animatedInstance;
    [HideInInspector] public GameObject finalInstance;

    public string animationTrigger = "Assemble";
}

public class PCBuildSequence : MonoBehaviour
{
    public List<BuildStep> steps = new List<BuildStep>();
    private int currentStep = -1;

    // 🔹 Reinicia la secuencia destruyendo todo
    public void ResetSequence()
    {
        foreach (var step in steps)
        {
            if (step.animatedInstance != null) Destroy(step.animatedInstance);
            if (step.finalInstance != null) Destroy(step.finalInstance);

            step.animatedInstance = null;
            step.finalInstance = null;
        }
        currentStep = -1;
        Debug.Log("🔄 Secuencia reseteada.");
    }

    // 🔹 Avanzar un paso en el ensamblaje
    public void NextStep()
    {
        // Cerrar paso previo
        if (currentStep >= 0 && currentStep < steps.Count)
        {
            var prevStep = steps[currentStep];

            // Destruir animado si estaba
            if (prevStep.animatedInstance != null)
            {
                Destroy(prevStep.animatedInstance);
                prevStep.animatedInstance = null;
            }

            // Instanciar versión final si existe
            if (prevStep.prefabFinal != null && prevStep.finalInstance == null)
            {
                prevStep.finalInstance = Instantiate(prevStep.prefabFinal, transform);
            }
        }

        // Avanzar al siguiente
        currentStep++;

        if (currentStep < steps.Count)
        {
            var step = steps[currentStep];

            if (step.prefabAnimated != null)
            {
                // Instanciar animación
                step.animatedInstance = Instantiate(step.prefabAnimated, transform);

                // Activar animator solo aquí
                Animator anim = step.animatedInstance.GetComponent<Animator>();
                if (anim != null)
                {
                    anim.enabled = true; // 🔹 Asegúrate que esté desactivado en el prefab
                    if (!string.IsNullOrEmpty(step.animationTrigger))
                    {
                        anim.ResetTrigger(step.animationTrigger);
                        anim.SetTrigger(step.animationTrigger);
                    }
                }
            }
        }
        else
        {
            Debug.Log("✅ Ensamblaje completo.");
        }
    }
}

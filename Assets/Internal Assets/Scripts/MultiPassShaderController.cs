using UnityEngine;
[ExecuteInEditMode]
public class MultiPassShaderController : MonoBehaviour
{
    public bool IsOn;
    [SerializeField] private Material[] passMaterials; // Массив материалов с шейдерами
    [SerializeField] private Material finalMaterial; // Материал plane
    [SerializeField] private Texture originalTexture; // Оригинальная текстура plane

    private RenderTexture[] renderTextures; // Массив рендер-текстур

    private void Start()
    {
        // Инициализация массива рендер-текстур
        renderTextures = new RenderTexture[passMaterials.Length];

        for (int i = 0; i < renderTextures.Length; i++)
        {
            renderTextures[i] = new RenderTexture(originalTexture.width, originalTexture.height, 0);
        }

        // Установка оригинальной текстуры в материал
        finalMaterial.mainTexture = originalTexture;
    }

    private void Update()
    {
        if (IsOn)
        {
            // Первый проход: применение первого шейдера к оригинальной текстуре
            Graphics.Blit(originalTexture, renderTextures[0], passMaterials[0]);

            // Последующие проходы: использование результата предыдущего прохода и применение следующего шейдера
            for (int i = 1; i < passMaterials.Length; i++)
            {
                passMaterials[i].SetTexture("_MainTex", renderTextures[i - 1]);
                Graphics.Blit(renderTextures[i - 1], renderTextures[i], passMaterials[i]);
            }

            // Применение конечной текстуры к материалу plane
            finalMaterial.mainTexture = renderTextures[passMaterials.Length - 1];
        }
        else
        {
            //оставляем только scanlines
            try
            {
                Graphics.Blit(originalTexture, renderTextures[3], passMaterials[3]);
                finalMaterial.mainTexture = renderTextures[3];
            }
            catch { }
        }
    }
}

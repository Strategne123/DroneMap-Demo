using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class Carousel : MonoBehaviour
{
    public Button leftButton; // Кнопка "влево"
    public Button rightButton; // Кнопка "вправо"
    [SerializeField] private float transitionDuration = 0.5f; // Продолжительность анимации
    public Transform indicatorParent; // Родитель для индикаторов
    public GameObject indicatorPrefab; // Префаб индикатора

    public int currentIndex{get;private set; }
    private Transform[] images;
    private Image[] indicators;
    public SettingsController settingsController;
    public eTypeSettings typeSetting;

    public void Init()
    {
        currentIndex = 0;
        // Получаем все изображения в дочерних объектах
        int childCount = transform.childCount;
        images = new Transform[childCount];
        indicators = new Image[childCount];

        for (int i = 0; i < childCount; i++)
        {
            images[i] = transform.GetChild(i);
            CanvasGroup canvasGroup = images[i].gameObject.AddComponent<CanvasGroup>();
            canvasGroup.alpha = i == currentIndex ? 1 : 0;
            images[i].gameObject.SetActive(i == currentIndex); // Активируем только первое изображение

            // Создаем индикаторы
            GameObject indicator = Instantiate(indicatorPrefab, indicatorParent);
            indicators[i] = indicator.GetComponent<Image>();
            indicators[i].color = new Color(indicators[i].color.r, indicators[i].color.g, indicators[i].color.b, i == currentIndex ? 1 : 0.5f);
        }
        UpdateButtonStates();
    }

    public void ShowPreviousImage()
    {
        int newIndex = (currentIndex - 1 + images.Length) % images.Length;
        AnimateTransition(images[currentIndex], images[newIndex], leftButton.transform.position, rightButton.transform.position);
        UpdateIndicators(newIndex);
        currentIndex = newIndex;
        UpdateButtonStates();
    }

    public void ShowNextImage()
    {
        int newIndex = (currentIndex + 1) % images.Length;
        AnimateTransition(images[currentIndex], images[newIndex], rightButton.transform.position, leftButton.transform.position);
        UpdateIndicators(newIndex);
        currentIndex = newIndex;
        UpdateButtonStates();
    }

    public void UpdateAll(int newIndex)
    {
        images[currentIndex].gameObject.SetActive(false);
        images[newIndex].gameObject.SetActive(true);
        CanvasGroup newCanvasGroup = images[newIndex].GetComponent<CanvasGroup>();
        newCanvasGroup.alpha = 1;
        UpdateIndicators(newIndex);
        currentIndex = newIndex;
        UpdateButtonStates();
    }

    private void AnimateTransition(Transform currentImage, Transform newImage, Vector3 enterPosition, Vector3 exitPosition)
    {
        CanvasGroup currentCanvasGroup = currentImage.GetComponent<CanvasGroup>();
        CanvasGroup newCanvasGroup = newImage.GetComponent<CanvasGroup>();
        newCanvasGroup.DOFade(1, transitionDuration);
        // Анимация выхода текущего изображения
        currentImage.DOMove(exitPosition, transitionDuration).OnComplete(() =>
        {
            if (!DOTween.IsTweening(currentImage))
            {
                currentImage.gameObject.SetActive(false);
            }
        });

        currentCanvasGroup.DOFade(0, transitionDuration);

        // Устанавливаем начальную позицию нового изображения и активируем его
        newImage.position = enterPosition;
        newImage.gameObject.SetActive(true);

        // Анимация входа нового изображения
        newImage.DOMove(transform.position, transitionDuration);
        newCanvasGroup.DOFade(1, transitionDuration);
    }

    private void UpdateIndicators(int newIndex)
    {
        for (int i = 0; i < indicators.Length; i++)
        {
            float targetAlpha = i == newIndex ? 1f : 0.5f;
            indicators[i].DOFade(targetAlpha, transitionDuration);
        }
    }

    private void UpdateButtonStates()
    {
        leftButton.interactable = currentIndex > 0;
        rightButton.interactable = currentIndex < images.Length - 1;
    }
}

using System.Collections;
using UnityEngine;

public class EchoPlatformSpawner : MonoBehaviour
{
    public EchoOnlyReceiver receiver;
    public GameObject platform;
    public float lifetime = 2f;

    private bool used;

    void Start()
    {
        if (platform != null)
            platform.SetActive(false);

        if (receiver != null)
            receiver.onActivated.AddListener(ShowPlatform);
    }

    void ShowPlatform()
    {
        if (used)
            return;

        used = true;

        if (platform != null)
            platform.SetActive(true);

        StartCoroutine(HideAfterLifetime());
    }

    IEnumerator HideAfterLifetime()
    {
        yield return new WaitForSeconds(lifetime);

        if (platform != null)
            platform.SetActive(false);
    }
}

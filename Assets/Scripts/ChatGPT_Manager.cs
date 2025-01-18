using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Newtonsoft.Json; // Use Newtonsoft.Json for proper serialization

public class ChatGPTManager : MonoBehaviour
{
    [SerializeField] private InputField chatInputField; // Assign in the Inspector
    [SerializeField] private Button submitButton;       // Assign in the Inspector
    private const string ApiUrl = "http://localhost:5000/generate";

    private void Start()
    {
        if (chatInputField == null)
        {
            Debug.LogError("Chat InputField is not assigned. Please assign it in the Inspector.");
        }

        if (submitButton == null)
        {
            Debug.LogError("Submit Button is not assigned. Please assign it in the Inspector.");
        }
        else
        {
            submitButton.onClick.AddListener(OnSubmit);
        }
    }

    private void OnSubmit()
    {
        string userInput = chatInputField.text.Trim();

        if (string.IsNullOrWhiteSpace(userInput))
        {
            Debug.LogError("Input cannot be empty.");
            return;
        }

        Debug.Log($"User Input: {userInput}");
        StartCoroutine(SendRequestToLocalLLM(userInput));

        chatInputField.text = string.Empty; // Clear the input field
    }

    private IEnumerator SendRequestToLocalLLM(string prompt)
    {
        // Correct JSON structure
        var requestBody = new { input = prompt };
        string jsonBody = JsonConvert.SerializeObject(requestBody); // Serialize properly with Newtonsoft.Json
        byte[] bodyData = Encoding.UTF8.GetBytes(jsonBody);

        using (UnityWebRequest request = new UnityWebRequest(ApiUrl, "POST"))
        {
            request.uploadHandler = new UploadHandlerRaw(bodyData);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"Response: {request.downloadHandler.text}");
            }
            else
            {
                Debug.LogError($"Request failed: {request.error}");
                Debug.LogError($"Response Text: {request.downloadHandler.text}");
            }
        }
    }
}


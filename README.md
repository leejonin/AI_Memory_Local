

# 📂 Unity-Python-Chat-Memory-System

A bridge system between **Unity** and **Python (Flask)** that provides long-term memory capabilities for AI NPCs. It stores conversation logs in structured JSON format and utilizes GPT-3.5 to summarize and search through past memories based on semantic relevance.

유니티와 파이썬(Flask)을 연결하여 AI NPC에게 장기 기억 능력을 부여하는 브릿지 시스템입니다. 대화 로그를 구조화된 JSON으로 저장하며, GPT-3.5를 사용하여 과거의 기억을 요약하고 의미 기반으로 검색할 수 있습니다.

---

## 🚀 Features
*   **Automated Server Management**: Unity automatically starts and stops the Python Flask server within the editor.[cite: 1]
*   **Conversation Logging**: Saves user inputs and AI responses to a nested JSON structure (Year/Month/Day/Hour/Minute).[cite: 2]
*   **GPT Summarization**: Automatically generates a one-sentence summary for each dialogue entry using OpenAI's API.[cite: 2]
*   **Semantic Memory Search**: Instead of simple keyword matching, it uses GPT to understand the context and retrieve relevant past memories.[cite: 2]
*   **Async/Await Support**: Provides modern C# `Task`-based API for seamless integration in Unity.[cite: 1]

---

## 🛠️ Tech Stack
*   **Client**: Unity (C#)
*   **Server**: Python (Flask)
*   **AI**: OpenAI API (GPT-3.5-turbo)
*   **Database**: JSON-based file storage[cite: 2]

---

## ⚙️ Setup

### 1. Python Environment
Install the required dependencies:
```bash
pip install flask openai
```

### 2. OpenAI API Key
In `Sever.py`, replace the placeholder with your actual API key:[cite: 2]
```python
client = openai.OpenAI(api_key="YOUR_OPENAI_API_KEY")
```

### 3. Unity Integration
1. Place `ServerCommunication.cs` in your Unity project.[cite: 1]
2. Ensure the `Sever.py` file is located at: `Assets/AI/ForChat/DateServer/Sever.py`.[cite: 1]
3. Attach the `ServerCommunication` script to a GameObject in your scene.

---

## 📖 How to Use (Code Examples)

### Saving Data (데이터 저장)
```csharp
// Saves current conversation to the JSON database
serverCommunication.SendCustomData("Hi, my name is Gemini.", "Hello! Nice to meet you.");
```

### Searching Memory (기억 검색)
You can use `async/await` to retrieve past information:[cite: 1]
```csharp
public async void SearchTest()
{
    var result = await serverCommunication.SearchMemory("What was my name?");
    if (result.found)
    {
        Debug.Log("GPT Answer: " + result.gptAnswer);
        // Output: "The user's name is Gemini."
    }
}
```

---

## 📝 상세 설명 (Korean)

### **서버 통신 방식**
이 시스템은 유니티의 `UnityWebRequest`를 사용하여 로컬 Flask 서버와 통신합니다. 유니티가 시작될 때 파이썬 프로세스를 백그라운드에서 자동으로 실행하며, 종료 시 프로세스를 함께 종료하도록 설계되었습니다.[cite: 1]

### **데이터 구조**
대화 내역은 `YYDate.Json` 파일에 계층적으로 저장됩니다.[cite: 2]
*   **연도 > 월 > 일 > 시 > 분** 순서로 데이터가 분류되어 체계적인 관리가 가능합니다.
*   각 엔트리는 원본 대화와 함께 GPT가 생성한 `summation`(요약)을 포함합니다.[cite: 2]

### **검색 메커니즘**
단순히 텍스트가 일치하는지 찾는 것이 아니라, GPT를 활용한 2단계 검색을 수행합니다:[cite: 2]
1.  **필터링**: 요약본 목록 중 키워드와 관련 있는 인덱스를 GPT가 선별합니다.
2.  **최종 답변**: 선별된 과거 대화 내용들을 종합하여 사용자의 질문에 대한 핵심 정보를 정리해 반환합니다.

---

## ⚠️ Requirements
*   Unity 2020.3 or higher
*   Python 3.x
*   Valid OpenAI API Key

---

## 🤝 Contribution
Contributions, issues, and feature requests are welcome!

---

**Note:** This project was developed as a bridge for AI-driven interactive characters.
**주의:** 이 프로젝트는 AI 기반 상호작용 캐릭터의 기억 시스템을 구현하기 위한 용도로 제작되었습니다.

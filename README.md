# AI_Memory_Local
🧠 AI_Memory_Local: GPT-Powered Local Conversation Memory Storage
This project is a robust system designed to bridge Unity-based AI agents with a persistent memory layer. It saves conversation history into local JSON files via a Python Flask server and leverages the OpenAI GPT API for automated summarization and semantic-based memory retrieval.

🚀 Key Features
Real-time Data Storage: Automatically synchronizes conversation data from Unity into a structured Date/Time hierarchy.

GPT Auto-Summarization: Utilizes gpt-3.5-turbo or gpt-4o-mini to generate concise, one-line summaries for every saved dialogue segment.

Semantic Search (/search): Goes beyond basic keyword matching. The system understands the context of past conversations to extract relevant memories and provide synthesized insights.

Auto-Recovery: On server startup, the system automatically scans historical data to identify and fill in any missing summaries.

🛠 Tech Stack
Backend: Python 3.x, Flask

AI: OpenAI API (GPT-3.5 / GPT-4)

Frontend Interface: Unity C# (ServerCommunication.cs)

Database: Local JSON File (YYDate.Json)

⚙️ Installation & Setup
Install Dependencies:

Bash
pip install flask openai


2.  **API Key Configuration**:
    Enter your OpenAI API key in the `OPENAI_API_KEY` variable at the top of `Sever.py`.

3.  **Run Server**:
    ```bash
    python Sever.py
    ```

## 📋 API Endpoints

| Method | Path | Description |
| :--- | :--- | :--- |
| **POST** | `/data` | Saves new conversation data and generates an automated summary. |
| **POST** | `/search` | Performs a semantic search of memories based on context/keywords. |
| **POST** | `/summation` | Regenerates a summary for a specific conversation timestamp. |
| **GET** | `/health` | Checks the server connectivity and status. |

# 🧠 AI_Memory_Local: GPT 기반 로컬 대화 기억 저장소

본 프로젝트는 Unity(C#) 클라이언트와 Python Flask 서버 간의 통신을 통해 사용자와 AI의 대화 내역을 로컬 JSON 파일에 저장하고, GPT API를 활용해 대화 요약 및 의미 기반 검색 기능을 제공하는 시스템입니다.

## 🚀 주요 기능
* **실시간 데이터 저장**: Unity에서 발생한 대화 데이터를 날짜/시간별 구조로 자동 저장.
* **GPT 자동 요약 (Summation)**: 대화가 저장될 때마다 `gpt-3.5-turbo` 또는 `gpt-4o-mini`를 사용하여 한 줄 요약을 생성.
* **의미 기반 검색 (/search)**: 단순 키워드 매칭을 넘어, GPT가 대화 맥락을 파악하여 관련 기억을 추출하고 답변을 생성.
* **자동 복구**: 서버 시작 시 요약이 비어 있는 과거 데이터를 전수 조사하여 자동으로 채움.

## 🛠 기술 스택
* **Backend**: Python 3.x, Flask
* **AI**: OpenAI API (GPT-3.5 / GPT-4)
* **Frontend Interface**: Unity C# (ServerCommunication.cs)
* **Database**: Local JSON File (`YYDate.Json`)

## ⚙️ 설치 및 설정
1. **의존성 설치**:
   ```bash
   pip install flask openai
API 키 설정:
Sever.py 상단의 OPENAI_API_KEY 부분에 본인의 OpenAI API 키를 입력하세요.

서버 실행:

Bash
python Sever.py

📋 API 엔드포인트 (API Endpoints)메서드경로설명POST/data새로운 대화 데이터를 저장하고 GPT를 통해 자동 요약을 생성합니다.POST/search키워드나 문맥을 바탕으로 과거 대화 기억에 대한 의미 기반 검색을 수행합니다.POST/summation특정 시간대의 대화 기록에 대해 요약본을 다시 생성합니다.GET/health서버의 연결 상태 및 정상 작동 여부를 확인합니다.

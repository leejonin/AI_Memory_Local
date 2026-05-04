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

Unity와 Python Flask를 연동하여 AI와의 대화 내역을 로컬에 저장하고, GPT를 이용해 똑똑하게 관리하는 프로젝트입니다. 단순한 로그 저장을 넘어 **자동 요약**과 **의미 기반 검색**을 통해 AI에게 '장기 기억'을 부여합니다.

## 🚀 주요 기능 (Key Features)

*   **💾 실시간 데이터 저장**: Unity에서 발생한 대화를 날짜/시간별 계층 구조로 JSON에 자동 기록합니다.
*   **📝 GPT 자동 요약**: 대화 저장 시 `gpt-3.5-turbo` 또는 `gpt-4o-mini`가 핵심 내용을 한 줄로 요약합니다.
*   **🔍 의미 기반 검색 (/search)**: 단순 단어 찾기가 아닌 문맥을 파악하는 검색으로 관련 기억을 추출합니다.
*   **🛠 자동 복구 시스템**: 서버 실행 시 요약이 누락된 과거 데이터를 전수 조사하여 자동으로 채워넣습니다.

## 🛠 기술 스택 (Tech Stack)

*   **Backend**: `Python 3.x`, `Flask`
*   **AI Engine**: `OpenAI API (GPT-3.5 / GPT-4)`
*   **Frontend**: `Unity C# (ServerCommunication.cs)`
*   **Database**: `Local JSON File (YYDate.Json)`

## ⚙️ 설치 및 설정 (Installation)

1️⃣ **의존성 설치**
```bash
pip install flask openai
```

2️⃣ **API 키 설정**
`Sever.py` 파일 상단의 `OPENAI_API_KEY` 변수에 본인의 OpenAI API 키를 입력하세요.

3️⃣ **서버 실행**
```bash
python Sever.py
```

## 📋 API 엔드포인트 (API Endpoints)

| 메서드 | 경로 | 설명 |
| :--- | :--- | :--- |
| **POST** | `/data` | 새로운 대화 데이터 저장 및 자동 요약 생성 |
| **POST** | `/search` | 키워드/문맥을 통한 의미 기반 대화 기억 검색 |
| **POST** | `/summation` | 특정 시간대 대화 데이터의 요약본 재생성 |
| **GET** | `/health` | 서버 연결 상태 및 상태 체크 |

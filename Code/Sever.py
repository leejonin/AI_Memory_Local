import json
import os
from flask import Flask, request, jsonify
from datetime import datetime
from threading import Thread
import time
import re
import openai  

# YOUR_OPENAI_API_KEY
client = openai.OpenAI(api_key="OPENAI_API_KEY")

app = Flask(__name__)

# JSON File Path
CURRENT_DIR = os.path.dirname(os.path.abspath(__file__))
JSON_FILE = os.path.join(CURRENT_DIR, "YYDate.Json")

# Server Status
server_running = False

def generate_summary(dv_input, nin_response):
    """
    Summarizes user input and AI response into one sentence using GPT API.
    """
    try:
        if not dv_input.strip() and not nin_response.strip():
            return "[Empty Conversation]"

        prompt = (
            f"Summarize the following conversation very briefly (one sentence) focusing on the core content.\n\n"
            f"Developer: {dv_input}\n"
            f"AI(Nina): {nin_response}\n\n"
            f"Format: [Dialogue Summary] Content"
        )

        response = client.chat.completions.create(
            model="gpt-3.5-turbo",
            messages=[
                {"role": "system", "content": "You are an assistant that concisely summarizes conversations. Deliver the core information without emojis."},
                {"role": "user", "content": prompt}
            ],
            max_tokens=100,
            temperature=0.5
        )

        summary = response.choices[0].message.content.strip()
        return summary

    except Exception as e:
        print(f"GPT Summary Generation Error: {e}")
        return f"[Summary Failed] {dv_input[:20]}..."


def collect_all_entries(data):
    """
    Traverses the full JSON to return all dialogue entries in a flat list.
    """
    entries = []

    def traverse(node):
        if isinstance(node, dict):
            if 'time' in node and 'dv_input' in node and 'nin_response' in node:
                entries.append({
                    'time': node.get('time', ''),
                    'dv_input': node.get('dv_input', ''),
                    'nin_response': node.get('nin_response', ''),
                    'summation': node.get('summation', '')
                })
                return
            for value in node.values():
                traverse(value)

    traverse(data)
    return entries


def search_by_keyword_gpt(keyword, entries):
    """
    Uses GPT to find dialogue entries semantically related to the keyword.
    """
    if not entries:
        return {"found": False, "matches": [], "reason": "No conversation data stored."}

    # Step 1: Extract related indices from summation list
    summation_list = "\n".join(
        [f"[{i}] {e['summation']}" for i, e in enumerate(entries)]
    )

    try:
        filter_prompt = (
            f"Here is a list of dialogue summaries:\n{summation_list}\n\n"
            f"Keyword: \"{keyword}\"\n\n"
            f"Respond ONLY with a JSON array of indices from the list that are semantically related to the keyword. "
            f"If none are related, return an empty array []. Example: [0, 2]"
        )

        filter_response = client.chat.completions.create(
            model="gpt-3.5-turbo",
            messages=[
                {"role": "system", "content": "You are an assistant that determines text relevance. Return ONLY a JSON array."},
                {"role": "user", "content": filter_prompt}
            ],
            max_tokens=100,
            temperature=0.0
        )

        raw = filter_response.choices[0].message.content.strip()
        indices = json.loads(raw)
        if not isinstance(indices, list):
            indices = []

    except Exception as e:
        print(f"GPT Filtering Error: {e}")
        indices = [i for i, e in enumerate(entries) if keyword.lower() in e['summation'].lower() or keyword.lower() in e['dv_input'].lower()]

    if not indices:
        return {
            "found": False,
            "matches": [],
            "reason": f"No conversation memory found related to '{keyword}'."
        }

    candidate_entries = [entries[i] for i in indices if 0 <= i < len(entries)]

    # Step 2: Final GPT summary generation for core info
    candidates_text = "\n\n".join([
        f"[Chat #{idx+1}]\nTime: {e['time']}\nSummary: {e['summation']}\n"
        f"User: {e['dv_input']}\nAI: {e['nin_response']}"
        for idx, e in enumerate(candidate_entries)
    ])

    try:
        answer_prompt = (
            f"These are dialogue records related to the keyword \"{keyword}\":\n\n{candidates_text}\n\n"
            f"Please summarize the core information about \"{keyword}\" from these dialogues concisely. "
            f"If information is sufficient, be specific; if not, state 'Some memories found but lack detailed information'."
        )

        answer_response = client.chat.completions.create(
            model="gpt-3.5-turbo",
            messages=[
                {"role": "system", "content": "You are an assistant that extracts information from dialogue records. Deliver core info without emojis."},
                {"role": "user", "content": answer_prompt}
            ],
            max_tokens=300,
            temperature=0.3
        )

        gpt_answer = answer_response.choices[0].message.content.strip()

    except Exception as e:
        print(f"GPT Answer Generation Error: {e}")
        gpt_answer = "Related dialogues found, but failed to generate a summary."

    return {
        "found": True,
        "matches": candidate_entries,
        "gpt_answer": gpt_answer,
        "reason": f"Found {len(candidate_entries)} conversation(s) related to '{keyword}'."
    }


def fill_summations():
    try:
        data = load_json_data()
        filled_count = 0
        
        def traverse_and_fill(node):
            nonlocal filled_count
            if isinstance(node, dict) and 'time' in node and 'dv_input' in node and 'nin_response' in node:
                if 'summation' not in node or not node['summation'].strip():
                    node['summation'] = generate_summary(node['dv_input'], node['nin_response'])
                    filled_count += 1
                return
            
            if isinstance(node, dict):
                for key, value in node.items():
                    traverse_and_fill(value)
        
        traverse_and_fill(data)
        if filled_count > 0:
            save_json_data(data)
        return filled_count
    except Exception as e:
        print(f"Error while filling summations: {e}")
        return 0

def load_json_data():
    try:
        with open(JSON_FILE, 'r', encoding='utf-8') as f:
            return json.load(f)
    except FileNotFoundError:
        return {}
    except json.JSONDecodeError as e:
        print(f"JSON Syntax Error: {e}. Initializing file.")
        return {}
    except Exception as e:
        print(f"JSON Load Error: {e}")
        return {}

def save_json_data(data):
    try:
        with open(JSON_FILE, 'w', encoding='utf-8') as f:
            json.dump(data, f, ensure_ascii=False, indent=2)
        return True
    except Exception as e:
        print(f"JSON Save Error: {e}")
        return False

def ensure_path(data, path_list):
    current = data
    for key in path_list[:-1]:
        if key not in current:
            current[key] = {}
        current = current[key]
    return current

@app.route('/data', methods=['POST'])
def receive_data():
    try:
        try:
            client_data = request.get_json(force=True)
        except Exception as parse_error:
            return jsonify({'success': False, 'error': f'Invalid JSON: {parse_error}'}), 400

        if client_data is None:
            return jsonify({'success': False, 'error': 'Empty JSON request.'}), 400
        
        if not all(key in client_data for key in ['time', 'dv_input', 'nin_response']):
            return jsonify({'success': False, 'error': 'Missing required fields'}), 400
        
        time_str = client_data['time']
        dv_input = client_data['dv_input']
        nin_response = client_data['nin_response']
        
        try:
            dt = datetime.strptime(time_str, '%Y-%m-%d-%H-%M-%S')
        except ValueError:
            return jsonify({'success': False, 'error': 'Invalid time format. Use YYYY-MM-DD-HH-mm-ss'}), 400
        
        year, month, day, hour, minute = dt.strftime('%Y'), dt.strftime('%m'), dt.strftime('%d'), dt.strftime('%H'), dt.strftime('%M')
        
        data = load_json_data()
        path = [year, month, day, hour, minute]
        parent = ensure_path(data, path)
        
        parent[path[-1]] = {
            'time': time_str,
            'dv_input': dv_input,
            'nin_response': nin_response,
            'summation': generate_summary(dv_input, nin_response)
        }
        
        if save_json_data(data):
            print(f"[{datetime.now()}] Data saved: {time_str}, Summary: {parent[path[-1]]['summation']}")
            return jsonify({'success': True, 'message': 'Data saved successfully', 'summation': parent[path[-1]]['summation']}), 200
        else:
            return jsonify({'success': False, 'error': 'Failed to save JSON'}), 500
            
    except Exception as e:
        print(f"Error: {e}")
        return jsonify({'success': False, 'error': str(e)}), 500

@app.route('/health', methods=['GET'])
def health_check():
    return jsonify({'status': 'running', 'timestamp': datetime.now().isoformat()}), 200

@app.route('/fill_summations', methods=['POST', 'GET'])
def fill_summations_endpoint():
    try:
        filled_count = fill_summations()
        return jsonify({
            'success': True,
            'message': f'{filled_count} summations generated.',
            'filled_count': filled_count
        }), 200
    except Exception as e:
        return jsonify({'success': False, 'error': str(e)}), 500

@app.route('/search', methods=['POST'])
def search_memory():
    try:
        req = request.get_json(force=True)
        keyword = req.get('keyword', '').strip()
        if not keyword:
            return jsonify({'success': False, 'error': 'Keyword field is required.'}), 400

        data = load_json_data()
        entries = collect_all_entries(data)
        result = search_by_keyword_gpt(keyword, entries)

        print(f"[{datetime.now()}] Search: '{keyword}' -> found={result['found']}, count={len(result['matches'])}")

        return jsonify({
            'success': True,
            'found': result['found'],
            'keyword': keyword,
            'reason': result['reason'],
            'gpt_answer': result.get('gpt_answer', ''),
            'matches': result['matches']
        }), 200

    except Exception as e:
        print(f"Search Error: {e}")
        return jsonify({'success': False, 'error': str(e)}), 500


def start_server(host='127.0.0.1', port=5000):
    global server_running
    server_running = True
    print(f"Server starting at http://{host}:{port}")
    fill_summations()
    app.run(host=host, port=port, debug=False, use_reloader=False)

if __name__ == '__main__':
    try:
        start_server()
    except KeyboardInterrupt:
        print("\nStopping server...")
from flask import Flask, request, jsonify
from transformers import pipeline

app = Flask(__name__)

# Load the model
try:
    generator = pipeline(
        "text-generation",
        model="gpt2",  # Replace with your desired model
        tokenizer="gpt2",
        pad_token_id=50256
    )
    print("Model loaded successfully.")
except Exception as e:
    print(f"Failed to load model: {e}")
    generator = None


@app.route("/generate", methods=["POST"])
def generate():
    if generator is None:
        return jsonify({"error": "Model not loaded. Please check the server logs."}), 500

    try:
        # Parse the JSON request
        data = request.get_json()

        if not data or "input" not in data:
            return jsonify({"error": "Invalid input. 'input' field is required."}), 400

        prompt = data.get("input", "").strip()
        if not prompt:
            return jsonify({"error": "Prompt cannot be empty."}), 400

        # Optional parameters with defaults
        max_length = data.get("max_length", 100)
        num_return_sequences = data.get("num_return_sequences", 1)

        # Generate the response
        outputs = generator(
            prompt,
            max_length=max_length,
            num_return_sequences=num_return_sequences,
            truncation=True
        )

        responses = [output["generated_text"] for output in outputs]

        # Log the prompt and response for debugging
        print(f"Prompt: {prompt}")
        print(f"Response: {responses[0]}")

        return jsonify({"response": responses[0]})

    except Exception as e:
        # Log the exception for debugging
        print(f"Error during generation: {e}")
        return jsonify({"error": str(e)}), 500


if __name__ == "__main__":
    app.run(host="0.0.0.0", port=5000)

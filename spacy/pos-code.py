import pandas as pd
from tqdm import tqdm
import os
import pyarrow.parquet as pq
import argparse
import spacy
import re
import json

def download_spacy_model():
    print("Downloading spaCy model...")
    spacy.cli.download("en_core_web_sm")
    print("spaCy model downloaded.")

def load_spacy_model():
    return spacy.load("en_core_web_sm")

def extract_words_with_positions(code):
    lines = code.split('\n')
    words_with_positions = []
    for line_num, line in enumerate(lines, 1):
        word_pattern = r'\b(?![0-9]+\b)\w+\b'
        for match in re.finditer(word_pattern, line):
            words_with_positions.append({
                "word": match.group(),
                "line": line_num,
                "start": match.start(),
                "end": match.end()
            })
    return words_with_positions

def pos_tag_words(words_with_positions, nlp):
    words = [word_info["word"] for word_info in words_with_positions]
    doc = nlp(" ".join(words))
    for token, word_info in zip(doc, words_with_positions):
        if token.pos_ != "SPACE":
            word_info["pos"] = token.pos_
    return words_with_positions

def process_parquet_file(input_file, output_file, nlp, chunk_size=1000):
    try:
        parquet_file = pq.ParquetFile(input_file)
        total_rows = parquet_file.metadata.num_rows
        print(f"File: {input_file}")
        print(f"Total rows: {total_rows}")
        
        if total_rows == 0:
            print("File is empty. Skipping.")
            return
        
        with open(output_file, 'w') as f:
            with tqdm(total=total_rows, desc="Processing") as pbar:
                for batch in parquet_file.iter_batches(batch_size=chunk_size):
                    chunk = batch.to_pandas()
                    
                    if 'content' not in chunk.columns:
                        print("Error: 'content' column not found in the Parquet file.")
                        print(f"Available columns: {chunk.columns}")
                        return
                    
                    for _, row in chunk.iterrows():
                        content = row['content']
                        if pd.notna(content):
                            words_with_positions = extract_words_with_positions(content)
                            pos_tagged_words = pos_tag_words(words_with_positions, nlp)
                            output = {
                                "original_code": content,
                                "tagged_words": pos_tagged_words
                            }
                            json.dump(output, f)
                            f.write('\n')
                    
                    pbar.update(len(chunk))

        
        print(f"Processed rows: {total_rows}")
    except Exception as e:
        import traceback
        print(f"Error processing file {input_file}: {str(e)}")
        print(traceback.format_exc())

def process_directory(input_dir, output_dir):
    os.makedirs(output_dir, exist_ok=True)
    
    nlp = load_spacy_model()
    
    for filename in os.listdir(input_dir):
        if filename.endswith('.parquet'):
            input_file = os.path.join(input_dir, filename)
            output_file = os.path.join(output_dir, f'pos_tagged_{os.path.splitext(filename)[0]}.json')
            print(f"\nProcessing {filename}...")
            process_parquet_file(input_file, output_file, nlp)
            print(f"Completed processing {filename}")

if __name__ == "__main__":
    parser = argparse.ArgumentParser(description="Process Parquet files containing C#/JavaScript code and apply POS tagging to words")
    parser.add_argument("--input", default="E:\\parquet\\code", help="Input directory containing Parquet files")
    parser.add_argument("--output", default="E:\\parquet\\codeoutput", help="Output directory for processed files")
    
    args = parser.parse_args()
    
    download_spacy_model()
    process_directory(args.input, args.output)
    print(f"\nAll files processed. Results saved in JSON format in the output directory.")
import pandas as pd
from tqdm import tqdm
import os
import pyarrow.parquet as pq
import argparse
import spacy

def download_spacy_model():
    print("Downloading spaCy model...")
    spacy.cli.download("en_core_web_sm")
    print("spaCy model downloaded.")

def load_spacy_model():
    return spacy.load("en_core_web_sm")

def pos_tag_text(text, nlp):
    if pd.isna(text):
        return ""
    doc = nlp(text)
    return ' '.join([token.pos_ for token in doc])

def pos_tag_texts(texts, nlp, batch_size=50):
    return [' '.join([token.pos_ for token in doc]) for doc in nlp.pipe(texts, batch_size=batch_size)]

def extract_sentences_with_pos(text, nlp):
    """
    Process the text to extract sentences and their corresponding POS tags.

    :param text: The input text to process.
    :param nlp: The spaCy NLP model.
    :return: A list of dictionaries, where each dictionary contains the sentence and its POS tags.
    """
    if pd.isna(text):
        return []
    
    doc = nlp(text)
    sentences_with_pos = []
    
    for sent in doc.sents:
        sentence_text = sent.text
        pos_tags = ' '.join([token.pos_ for token in sent])
        sentences_with_pos.append({
            'sentence': sentence_text,
            'pos_tags': pos_tags
        })
    
    return sentences_with_pos

def process_parquet_file(input_file, output_file, output_format='json', chunk_size=1000):
    try:
        print("Loading spaCy model...")
        nlp = spacy.load("en_core_web_sm")  # Ensure parser is enabled for sentence segmentation
        print("spaCy model loaded.")
        
        parquet_file = pq.ParquetFile(input_file)
        total_rows = parquet_file.metadata.num_rows
        print(f"File: {input_file}")
        print(f"Total rows: {total_rows}")
        print(f"Schema: {parquet_file.schema}")
        
        if total_rows == 0:
            print("File is empty. Skipping.")
            return
        
        all_processed_chunks = []
        with tqdm(total=total_rows, desc="Processing") as pbar:
            for batch in parquet_file.iter_batches(batch_size=chunk_size):
                chunk = batch.to_pandas()
                
                if 'text' not in chunk.columns:
                    print("Error: 'text' column not found in the Parquet file.")
                    print(f"Available columns: {chunk.columns}")
                    return
                
                print(f"Processing chunk of size {len(chunk)}...")
                chunk['sentences_with_pos'] = chunk['text'].apply(lambda x: extract_sentences_with_pos(x, nlp))

                # Exclude the 'text' field before appending
                chunk = chunk.drop(columns=['text'])

                all_processed_chunks.append(chunk)
                
                pbar.update(len(chunk))
        
        print("Combining processed chunks...")
        final_df = pd.concat(all_processed_chunks, ignore_index=True)
        
        print(f"Writing to output file in {output_format} format...")
        if output_format == 'csv':
            # Flattening the sentences_with_pos to save in CSV format
            final_df = final_df.explode('sentences_with_pos')
            final_df = pd.concat([final_df.drop(['sentences_with_pos'], axis=1),
                                  final_df['sentences_with_pos'].apply(pd.Series)], axis=1)
            final_df.to_csv(output_file, index=False)
        elif output_format == 'json':
            final_df.to_json(output_file, orient='records', lines=True)
        elif output_format == 'parquet':
            final_df.to_parquet(output_file, engine='pyarrow', index=False)
        else:
            raise ValueError(f"Unsupported output format: {output_format}")
        
        print(f"Processed rows: {total_rows}")
    except Exception as e:
        import traceback
        print(f"Error processing file {input_file}: {str(e)}")
        print(traceback.format_exc())



def process_directory(input_dir, output_dir, output_format):
    os.makedirs(output_dir, exist_ok=True)
    
    for filename in os.listdir(input_dir):
        if filename.endswith('.parquet'):
            input_file = os.path.join(input_dir, filename)
            output_file = os.path.join(output_dir, f'pos_tagged_{os.path.splitext(filename)[0]}.{output_format}')
            print(f"\nProcessing {filename}...")
            process_parquet_file(input_file, output_file, output_format)
            print(f"Completed processing {filename}")

if __name__ == "__main__":
    parser = argparse.ArgumentParser(description="Process Parquet files and apply POS tagging using spaCy")
    parser.add_argument("--input", default="E:\\parquet\\files", help="Input directory containing Parquet files")
    parser.add_argument("--output", default="E:\\parquet\\output", help="Output directory for processed files")
    parser.add_argument("--format", choices=['csv', 'json', 'parquet'], default='json', help="Output file format (default: csv)")
    
    args = parser.parse_args()
    
    download_spacy_model()
    process_directory(args.input, args.output, args.format)
    print(f"\nAll files processed. Results saved in {args.format} format in the output directory.")
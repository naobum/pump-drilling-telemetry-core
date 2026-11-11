import { useRef } from 'react';

interface FileDropzoneProps {
    onFileLoaded: (content: string, fileName: string) => void;
}

export function FileDropzone({ onFileLoaded }: FileDropzoneProps) {
    const fileInputRef = useRef<HTMLInputElement>(null);

    const handleFileChange = (event: React.ChangeEvent<HTMLInputElement>) => {
        const file = event.target.files?.[0];
        if (!file) return;

        const reader = new FileReader();
        reader.onload = (e) => {
            const content = e.target?.result as string;
            onFileLoaded(content, file.name);
        };
        reader.readAsText(file);

        // Сброс, чтобы можно было загрузить тот же файл повторно
        if (fileInputRef.current) fileInputRef.current.value = '';
    };

    const handleDrop = (event: React.DragEvent<HTMLDivElement>) => {
        event.preventDefault();
        const file = event.dataTransfer.files?.[0];
        if (!file) return;

        const reader = new FileReader();
        reader.onload = (e) => {
            const content = e.target?.result as string;
            onFileLoaded(content, file.name);
        };
        reader.readAsText(file);
    };

    return (
        <div
            onDragOver={(e) => e.preventDefault()}
            onDrop={handleDrop}
            style={{
                border: '2px dashed #ccc',
                padding: '40px',
                textAlign: 'center',
                borderRadius: '8px',
                cursor: 'pointer',
                backgroundColor: '#fafafa'
            }}
            onClick={() => fileInputRef.current?.click()}
        >
            <input
                type="file"
                accept=".json"
                ref={fileInputRef}
                onChange={handleFileChange}
                style={{ display: 'none' }}
            />
            <p>Нажмите или перетащите файл JSON сюда</p>
        </div>
    );
}

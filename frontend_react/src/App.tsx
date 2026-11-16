import { useState, useEffect } from 'react';
import { FileDropzone } from './components/FileDropzone';
import { PumpChart } from './components/PumpChart';
import { buildChartData } from './lib/chartData';
import { parsePumpFile } from './lib/parse';
import type { AnalysisResult } from './types/pump';
import type { ChartPoint } from './lib/chartData';

function App() {
  const [fileName, setFileName] = useState<string | null>(() => {
    return localStorage.getItem('pump_fileName');
  });

  const [error, setError] = useState<{ category: string; message: string } | null>(null);

  const [totals, setTotals] = useState<AnalysisResult | null>(() => {
    try {
      const saved = localStorage.getItem('pump_totals');
      return saved ? JSON.parse(saved) : null;
    } catch {
      return null;
    }
  });

  const [chartData, setChartData] = useState<ChartPoint[]>(() => {
    try {
      const saved = localStorage.getItem('pump_chartData');
      return saved ? JSON.parse(saved) : [];
    } catch {
      return [];
    }
  });

  const [isLoading, setIsLoading] = useState(false);

  useEffect(() => {
    if (fileName) localStorage.setItem('pump_fileName', fileName);
    else localStorage.removeItem('pump_fileName');
  }, [fileName]);

  useEffect(() => {
    if (totals) localStorage.setItem('pump_totals', JSON.stringify(totals));
    else localStorage.removeItem('pump_totals');
  }, [totals]);

  useEffect(() => {
    if (chartData.length > 0) localStorage.setItem('pump_chartData', JSON.stringify(chartData));
    else localStorage.removeItem('pump_chartData');
  }, [chartData]);

  const handleFileLoaded = async (content: string, name: string) => {
    setFileName(name);
    setError(null);
    setTotals(null);
    setChartData([]);
    setIsLoading(true);

    try {
      const rawJson = JSON.parse(content);

      // 1. ЛОКАЛЬНАЯ ВАЛИДАЦИЯ
      const localCheck = parsePumpFile(rawJson);
      if (localCheck.status === 'format_error') {
        setError(localCheck.error);
        setIsLoading(false);
        return;
      }

      // 2. ОТПРАВКА НА БЭКЕНД
      const response = await fetch('/api/pump/work-session', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(rawJson),
      });

      if (!response.ok) {
        const errorData = await response.json();
        let errorMessage = "Ошибка валидации на сервере.";
        if (errorData.errors) errorMessage = Object.values(errorData.errors).flat().join(' ');
        else if (errorData.title) errorMessage = errorData.title;

        setError({ category: 'backend_error', message: errorMessage });
        return;
      }

      const backendData = await response.json();

      setTotals({
        sample_count: localCheck.file.samples.length,
        span_s: backendData.span_s,
        working_s: backendData.working_s,
        stopped_s: backendData.stopped_s,
        gap_s: backendData.gap_s,
        invalid_s: backendData.invalid_s,
        unknown_s: backendData.unknown_s,
        modes_s: backendData.modes_s,
      });

      setChartData(buildChartData(localCheck.file.samples));

    } catch (err) {
      setError({
        category: 'network_error',
        message: "Сервер недоступен или файл не является корректным JSON."
      });
    } finally {
      setIsLoading(false);
    }
  };

  // Утилита для скачивания JSON
  const downloadJSON = (data: object, originalFileName: string) => {
    const jsonContent = JSON.stringify(data, null, 2);
    const blob = new Blob([jsonContent], { type: 'application/json;charset=utf-8;' });
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.setAttribute('download', `resultat_${originalFileName}`);
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    URL.revokeObjectURL(url);
  };

  // Экспорт: либо ошибка, либо результат
  const handleExportJSON = () => {
    if (!fileName) return;

    // Вариант 1: Экспорт ошибки
    if (error) {
      const exportData = {
        schema_version: "1.0",
        duration_unit: "seconds",
        results: {
          [fileName]: {
            status: "format_error",
            category: error.category,
            message_example: error.message,
            totals: null
          }
        }
      };
      downloadJSON(exportData, fileName);
      return;
    }

    // Вариант 2: Экспорт результата
    if (totals) {
      const exportData = {
        schema_version: "1.0",
        duration_unit: "seconds",
        results: {
          [fileName]: {
            status: "ok",
            sample_count: totals.sample_count,
            span_s: totals.span_s,
            working_s: totals.working_s,
            stopped_s: totals.stopped_s,
            gap_s: totals.gap_s,
            invalid_s: totals.invalid_s,
            unknown_s: totals.unknown_s,
            modes_s: {
              low: totals.modes_s.low,
              normal: totals.modes_s.normal,
              high: totals.modes_s.high
            }
          }
        }
      };
      downloadJSON(exportData, fileName);
    }
  };

  const handleReset = () => {
    setFileName(null);
    setError(null);
    setTotals(null);
    setChartData([]);
  };

  return (
    <div style={{ maxWidth: '1400px', margin: '0 auto', padding: '20px', fontFamily: 'system-ui, sans-serif' }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '20px' }}>
        <h1 style={{ margin: 0 }}>Анализатор насоса</h1>
        {(totals || error) && (
          <button
            onClick={handleReset}
            style={{ padding: '8px 16px', backgroundColor: '#f44336', color: 'white', border: 'none', borderRadius: '4px', cursor: 'pointer' }}
          >
            Сбросить
          </button>
        )}
      </div>

      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(500px, 1fr))', gap: '30px', alignItems: 'start' }}>

        <div>
          <FileDropzone onFileLoaded={handleFileLoaded} />
          {fileName && <p><strong>Загруженный файл:</strong> {fileName}</p>}

          {isLoading && (
            <div style={{ padding: '20px', textAlign: 'center', color: '#666' }}>
              Идет обработка данных на сервере...
            </div>
          )}

          {/* БЛОК ОШИБКИ с кнопкой экспорта */}
          {error && (
            <div style={{ backgroundColor: '#fee', color: '#c00', padding: '15px', borderRadius: '5px', marginTop: '20px', border: '1px solid #fcc' }}>
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', gap: '15px' }}>
                <div>
                  <h3 style={{ margin: '0 0 10px 0' }}>Ошибка ({error.category})</h3>
                  <p style={{ margin: 0 }}>{error.message}</p>
                  <p style={{ margin: '10px 0 0 0', fontSize: '0.9em' }}><i>Итоговые значения не могут быть рассчитаны.</i></p>
                </div>
                <button
                  onClick={handleExportJSON}
                  style={{
                    padding: '8px 16px', backgroundColor: '#c00', color: 'white',
                    border: 'none', borderRadius: '4px', cursor: 'pointer',
                    fontSize: '14px', fontWeight: 'bold', whiteSpace: 'nowrap'
                  }}
                >
                  Сохранить ошибку (JSON)
                </button>
              </div>
            </div>
          )}

          {/* БЛОК РЕЗУЛЬТАТОВ */}
          {totals && (
            <div style={{ marginTop: '20px', backgroundColor: '#f0f4ff', padding: '20px', borderRadius: '8px', border: '1px solid #d0d9f0' }}>
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '15px' }}>
                <h2 style={{ margin: 0 }}>Результаты анализа</h2>
                <button
                  onClick={handleExportJSON}
                  style={{ padding: '8px 16px', backgroundColor: '#4CAF50', color: 'white', border: 'none', borderRadius: '4px', cursor: 'pointer', fontSize: '14px', fontWeight: 'bold' }}
                >
                  Сохранить результат (JSON)
                </button>
              </div>

              <ul style={{ listStyleType: 'none', padding: 0, display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '10px', margin: 0 }}>
                <li><strong>Отсчеты:</strong> {totals.sample_count}</li>
                <li><strong>Общая длительность (span):</strong> {totals.span_s} с</li>
                <li><strong>Работа:</strong> {totals.working_s} с</li>
                <li><strong>Остановка:</strong> {totals.stopped_s} с</li>
                <li><strong>Пропуски (gap):</strong> {totals.gap_s} с</li>
                <li><strong>Недостоверные:</strong> {totals.invalid_s} с</li>
                <li><strong>Неизвестно (gap + недостоверные):</strong> {totals.unknown_s} с</li>
              </ul>

              <h3 style={{ marginTop: '20px', marginBottom: '10px' }}>Режимы работы</h3>
              <ul style={{ listStyleType: 'none', padding: 0, display: 'flex', gap: '20px', margin: 0 }}>
                <li><strong>Низкий (low):</strong> {totals.modes_s.low} с</li>
                <li><strong>Нормальный (normal):</strong> {totals.modes_s.normal} с</li>
                <li><strong>Высокий (high):</strong> {totals.modes_s.high} с</li>
              </ul>
            </div>
          )}
        </div>

        <div style={{ backgroundColor: '#fff', padding: '20px', borderRadius: '8px', border: '1px solid #eee', minHeight: '450px' }}>
          <h2 style={{ marginTop: 0 }}>Временная визуализация</h2>
          {chartData.length > 0 ? (
            <PumpChart data={chartData} />
          ) : (
            <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'center', height: '350px', color: '#999' }}>
              Загрузите файл, чтобы увидеть график
            </div>
          )}
        </div>
      </div>
    </div>
  );
}

export default App;
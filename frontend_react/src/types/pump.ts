// src/types/pump.ts

/** 
 * Структура одного измерения, которую мы отправляем на бэкенд.
 * (Соответствует MeasureDto.cs на бэкенде)
 */
export interface Sample {
    t_s: number;
    pressure_mpa: number | null;
    rpm: number | null;
    valid: boolean;
}

/** 
 * Структура запроса, которую ждет бэкенд.
 * (Соответствует CreateWorkSessionRequest.cs на бэкенде)
 */
export interface WorkSessionRequest {
    schema_version: string;
    dataset_id: string;
    synthetic: boolean;
    samples: Sample[];
}

/** 
 * Режимы работы работающего насоса.
 */
export type Mode = 'low' | 'normal' | 'high';

/** 
 * Ответ, который присылает бэкенд после расчета.
 * (Соответствует WorkSessionSummaryResponse.cs на бэкенде)
 */
export interface WorkSessionSummaryResponse {
    working_s: number;
    stopped_s: number;
    gap_s: number;
    invalid_s: number;
    unknown_s: number;
    span_s: number;
    modes_s: Record<Mode, number>;
}

/** 
 * Расширенный тип для удобства фронтенда.
 * Мы добавляем sample_count, так как бэкенд его не присылает, 
 * но он нужен для отображения в интерфейсе.
 */
export interface AnalysisResult extends WorkSessionSummaryResponse {
    sample_count: number;
}


/** Необработанный образец данных в том виде, в котором он представлен в JSON. */
export interface RawSample {
    t_s: unknown;
    pressure_mpa: unknown;
    rpm: unknown;
    valid: unknown;
}

/** Необработанный файл до валидации. */
export interface RawPumpFile {
    schema_version?: unknown;
    dataset_id?: unknown;
    synthetic?: unknown;
    samples?: unknown;
}


/** Валидированный файл. */
export interface PumpFile {
    schema_version: '1.0';
    dataset_id: string;
    synthetic: true;
    samples: Sample[];
}

/** Категории ошибок формата, соответствующие expected_results.json. */
export type FormatErrorCategory =
    | 'missing_field'
    | 'wrong_type'
    | 'wrong_version'
    | 'wrong_structure'
    | 'non_increasing_time';

export interface FormatError {
    category: FormatErrorCategory;
    /** Читаемое сообщение, объясняющее причину и местоположение ошибки. */
    message: string;
}

/** Результат парсинга: либо валидный файл, либо ошибка формата. */
export type ParseResult =
    | { status: 'ok'; file: PumpFile }
    | { status: 'format_error'; error: FormatError };


/** Результат анализа интервалов. Все длительности в секундах. */
export interface AnalysisResult {
    sample_count: number;
    span_s: number;
    working_s: number;
    stopped_s: number;
    gap_s: number;
    invalid_s: number;
    unknown_s: number; // gap_s + invalid_s
    modes_s: Record<Mode, number>;
}
import type { FormatError, ParseResult, RawPumpFile, RawSample, Sample } from '../types/pump';

function isFiniteNumber(value: unknown): value is number {
    return typeof value === 'number' && Number.isFinite(value);
}

function isNullableNumber(value: unknown): value is number | null {
    return value === null || isFiniteNumber(value);
}

export function parsePumpFile(raw: unknown): ParseResult {
    if (typeof raw !== 'object' || raw === null) {
        return fail('wrong_structure', 'Корневой элемент файла должен быть объектом JSON.');
    }

    const file = raw as RawPumpFile;

    // 1. Обязательные поля корня
    if (file.schema_version === undefined) return fail('missing_field', "Отсутствует обязательное поле: 'schema_version'.");
    if (file.schema_version !== '1.0') return fail('wrong_version', `Неподдерживаемая версия схемы: ${String(file.schema_version)}.`);
    if (file.dataset_id === undefined) return fail('missing_field', "Отсутствует обязательное поле: 'dataset_id'.");
    if (typeof file.dataset_id !== 'string') return fail('wrong_type', "'dataset_id' должен быть строкой.");
    if (file.synthetic === undefined) return fail('missing_field', "Отсутствует обязательное поле: 'synthetic'.");
    if (file.synthetic !== true) return fail('wrong_type', "'synthetic' должен быть логическим значением true.");
    if (file.samples === undefined) return fail('missing_field', "Отсутствует обязательное поле: 'samples'.");
    if (!Array.isArray(file.samples)) return fail('wrong_structure', "'samples' должен быть массивом.");

    const samples: Sample[] = [];
    let previousT = -Infinity;

    // 2. Проверка каждой записи
    for (let i = 0; i < file.samples.length; i++) {
        const rawSample = file.samples[i] as RawSample;
        const prefix = `samples[${i}]`;

        if (typeof rawSample !== 'object' || rawSample === null) {
            return fail('wrong_structure', `${prefix} должен быть объектом.`);
        }

        if (rawSample.t_s === undefined) return fail('missing_field', `В ${prefix} отсутствует поле 't_s'.`);
        if (!isFiniteNumber(rawSample.t_s) || rawSample.t_s < 0) {
            return fail('wrong_type', `${prefix}.t_s должен быть конечным числом >= 0.`);
        }
        if (rawSample.t_s <= previousT) {
            return fail('non_increasing_time', `${prefix}.t_s повторяет или предшествует предыдущей метке времени.`);
        }
        previousT = rawSample.t_s;

        if (rawSample.pressure_mpa === undefined) return fail('missing_field', `В ${prefix} отсутствует поле 'pressure_mpa'.`);
        if (!isNullableNumber(rawSample.pressure_mpa)) {
            return fail('wrong_type', `${prefix}.pressure_mpa должен быть конечным числом или null.`);
        }

        if (rawSample.rpm === undefined) return fail('missing_field', `В ${prefix} отсутствует поле 'rpm'.`);
        if (!isNullableNumber(rawSample.rpm)) {
            return fail('wrong_type', `${prefix}.rpm должен быть конечным числом или null.`);
        }

        if (rawSample.valid === undefined) return fail('missing_field', `В ${prefix} отсутствует поле 'valid'.`);
        if (typeof rawSample.valid !== 'boolean') {
            return fail('wrong_type', `${prefix}.valid должен быть логическим значением.`);
        }

        samples.push({
        t_s: rawSample.t_s,
        pressure_mpa: rawSample.pressure_mpa,
        rpm: rawSample.rpm,
        valid: rawSample.valid,
        });
    }

    return {
        status: 'ok',
        file: {
        schema_version: '1.0',
        dataset_id: file.dataset_id,
        synthetic: true,
        samples,
        },
    };
}

function fail(category: FormatError['category'], message: string): ParseResult {
    return { status: 'format_error', error: { category, message } };
}
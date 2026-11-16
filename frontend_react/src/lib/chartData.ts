import type { Sample } from '../types/pump';

export interface ChartPoint {
    t_s: number;
    pressure_mpa: number | null;
    rpm: number | null;
    isGap: boolean;
}

export function buildChartData(samples: Sample[]): ChartPoint[] {
    const data: ChartPoint[] = [];

    for (let i = 0; i < samples.length; i++) {
        const current = samples[i];
        const previous = i > 0 ? samples[i - 1] : null;

        // Если это первая точка, добавляем её как есть
        if (!previous) {
            data.push({
                t_s: current.t_s,
                pressure_mpa: current.pressure_mpa,
                rpm: current.rpm,
                isGap: false,
            });
            continue;
        }

        const dt = current.t_s - previous.t_s;
        const isGap = dt > 1.5;

        if (isGap) {
            // Вставляем "фантомную" точку чуть раньше текущей,
            // чтобы заставить Recharts разорвать линию графика.
            data.push({
                t_s: previous.t_s + 0.001, // Чуть позже предыдущей точки
                pressure_mpa: null,
                rpm: null,
                isGap: true,
            });
        }

        // Добавляем текущую точку
        data.push({
            t_s: current.t_s,
            pressure_mpa: current.pressure_mpa,
            rpm: current.rpm,
            isGap: false,
        });
    }

    return data;
}
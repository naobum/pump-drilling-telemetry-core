import {
    ComposedChart,
    Line,
    XAxis,
    YAxis,
    CartesianGrid,
    Tooltip,
    Legend,
    ResponsiveContainer,
    ReferenceArea
} from 'recharts';
import type { ChartPoint } from '../lib/chartData';

interface PumpChartProps {
    data: ChartPoint[];
}

export function PumpChart({ data }: PumpChartProps) {
    if (data.length === 0) return null;

    // Ищем зоны разрывов, чтобы закрасить их серым
    const gaps: { x1: number; x2: number }[] = [];
    for (let i = 0; i < data.length - 1; i++) {
        if (data[i].isGap) {
            // Разрыв начинается в конце предыдущей точки и заканчивается в текущей
            // Ищем реальную предыдущую точку
            let prevRealIndex = i - 1;
            while (prevRealIndex >= 0 && data[prevRealIndex].isGap) prevRealIndex--;

            if (prevRealIndex >= 0) {
                gaps.push({
                    x1: data[prevRealIndex].t_s,
                    x2: data[i + 1].t_s
                });
            }
        }
    }

    return (
        <div style={{ width: '100%', height: 400, marginTop: '20px' }}>
            <ResponsiveContainer>
                <ComposedChart data={data} margin={{ top: 20, right: 30, left: 20, bottom: 20 }}>
                    <CartesianGrid strokeDasharray="3 3" />
                    <XAxis
                        dataKey="t_s"
                        type="number"
                        domain={['dataMin', 'dataMax']}
                        tickFormatter={(val) => `${val} с`}
                    />

                    {/* Левая ось Y: Давление */}
                    <YAxis
                        yAxisId="left"
                        orientation="left"
                        stroke="#8884d8"
                        label={{ value: 'Давление (МПа)', angle: -90, position: 'insideLeft' }}
                    />

                    {/* Правая ось Y: Обороты */}
                    <YAxis
                        yAxisId="right"
                        orientation="right"
                        stroke="#82ca9d"
                        label={{ value: 'Обороты (об/мин)', angle: 90, position: 'insideRight' }}
                    />

                    <Tooltip
                        labelFormatter={(label) => `Время: ${label} с`}
                        formatter={(value: any, name: any) => {
                            if (value === null) return ['-', name];
                            return [value.toFixed(2), name];
                        }}
                    />
                    <Legend />

                    {/* Закрашивание зон разрывов */}
                    {gaps.map((gap, index) => (
                        <ReferenceArea
                            key={index}
                            yAxisId="left"
                            x1={gap.x1}
                            x2={gap.x2}
                            fill="#ccc"
                            fillOpacity={0.3}
                        />
                    ))}

                    <Line
                        yAxisId="left"
                        type="monotone"
                        dataKey="pressure_mpa"
                        name="Давление (МПа)"
                        stroke="#8884d8"
                        strokeWidth={2}
                        dot={false}
                        connectNulls={false} // ОЧЕНЬ ВАЖНО: разрывает линию на пропусках
                        activeDot={{ r: 6 }}
                    />
                    <Line
                        yAxisId="right"
                        type="monotone"
                        dataKey="rpm"
                        name="Обороты (об/мин)"
                        stroke="#82ca9d"
                        strokeWidth={2}
                        dot={false}
                        connectNulls={false} // ОЧЕНЬ ВАЖНО: разрывает линию на пропусках
                        activeDot={{ r: 6 }}
                    />
                </ComposedChart>
            </ResponsiveContainer>
        </div>
    );
}
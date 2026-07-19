import React from 'react';
import { LineChart, Line, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, Legend } from 'recharts';

const LineChartComponent = ({ data, dataKey1, dataKey2, color1 = '#4f8ef7', color2 = '#10b981' }) => {
  return (
    <ResponsiveContainer width="100%" height={280}>
      <LineChart data={data} margin={{ top: 10, right: 10, left: 0, bottom: 0 }}>
        <CartesianGrid strokeDasharray="3 3" stroke="#1e2d45" />
        <XAxis 
          dataKey="label" 
          stroke="#64748b" 
          style={{ fontSize: '0.75rem' }}
          tick={{ fill: '#64748b' }}
        />
        <YAxis 
          stroke="#64748b" 
          style={{ fontSize: '0.75rem' }}
          tick={{ fill: '#64748b' }}
        />
        <Tooltip 
          contentStyle={{
            backgroundColor: '#1a2235',
            border: '1px solid #2a3f5f',
            borderRadius: '8px',
            fontSize: '0.85rem'
          }}
          labelStyle={{ color: '#f1f5f9' }}
          itemStyle={{ color: '#94a3b8' }}
        />
        {dataKey2 && <Legend wrapperStyle={{ fontSize: '0.8rem', color: '#94a3b8' }} />}
        <Line 
          type="monotone" 
          dataKey={dataKey1} 
          stroke={color1} 
          strokeWidth={2}
          dot={{ r: 4 }}
          activeDot={{ r: 6 }}
        />
        {dataKey2 && (
          <Line 
            type="monotone" 
            dataKey={dataKey2} 
            stroke={color2} 
            strokeWidth={2}
            dot={{ r: 4 }}
            activeDot={{ r: 6 }}
          />
        )}
      </LineChart>
    </ResponsiveContainer>
  );
};

export default LineChartComponent;

import React from 'react';
import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, Legend } from 'recharts';

const BarChartComponent = ({ data, dataKey1, dataKey2, color1 = '#10b981', color2 = '#ef4444' }) => {
  return (
    <ResponsiveContainer width="100%" height={280}>
      <BarChart data={data} margin={{ top: 10, right: 10, left: 0, bottom: 0 }}>
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
        <Bar dataKey={dataKey1} fill={color1} radius={[4, 4, 0, 0]} />
        {dataKey2 && <Bar dataKey={dataKey2} fill={color2} radius={[4, 4, 0, 0]} />}
      </BarChart>
    </ResponsiveContainer>
  );
};

export default BarChartComponent;

import React from 'react';
import { AreaChart, Area, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer } from 'recharts';

const AreaChartComponent = ({ data, dataKey1, dataKey2, color1 = '#4f8ef7', color2 = '#ef4444' }) => {
  return (
    <ResponsiveContainer width="100%" height={280}>
      <AreaChart data={data} margin={{ top: 10, right: 10, left: 0, bottom: 0 }}>
        <defs>
          <linearGradient id="color1" x1="0" y1="0" x2="0" y2="1">
            <stop offset="5%" stopColor={color1} stopOpacity={0.3}/>
            <stop offset="95%" stopColor={color1} stopOpacity={0}/>
          </linearGradient>
          {dataKey2 && (
            <linearGradient id="color2" x1="0" y1="0" x2="0" y2="1">
              <stop offset="5%" stopColor={color2} stopOpacity={0.3}/>
              <stop offset="95%" stopColor={color2} stopOpacity={0}/>
            </linearGradient>
          )}
        </defs>
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
        <Area 
          type="monotone" 
          dataKey={dataKey1} 
          stroke={color1} 
          strokeWidth={2}
          fillOpacity={1} 
          fill="url(#color1)" 
        />
        {dataKey2 && (
          <Area 
            type="monotone" 
            dataKey={dataKey2} 
            stroke={color2} 
            strokeWidth={2}
            fillOpacity={1} 
            fill="url(#color2)" 
          />
        )}
      </AreaChart>
    </ResponsiveContainer>
  );
};

export default AreaChartComponent;

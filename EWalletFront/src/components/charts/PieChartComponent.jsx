import React from 'react';
import { PieChart, Pie, Cell, ResponsiveContainer, Tooltip, Legend } from 'recharts';

const COLORS = ['#4f8ef7', '#10b981', '#f59e0b', '#ef4444', '#7c3aed', '#06b6d4'];

const PieChartComponent = ({ data }) => {
  return (
    <ResponsiveContainer width="100%" height={280}>
      <PieChart>
        <Pie
          data={data}
          cx="50%"
          cy="50%"
          labelLine={false}
          label={({ label, percent }) => `${label}: ${(percent * 100).toFixed(0)}%`}
          outerRadius={80}
          fill="#8884d8"
          dataKey="value"
        >
          {data.map((entry, index) => (
            <Cell key={`cell-${index}`} fill={COLORS[index % COLORS.length]} />
          ))}
        </Pie>
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
        <Legend 
          wrapperStyle={{ fontSize: '0.75rem', color: '#94a3b8' }}
          iconType="circle"
        />
      </PieChart>
    </ResponsiveContainer>
  );
};

export default PieChartComponent;

import React from 'react';
import { describe, it, expect } from 'vitest';
import { render, screen } from '@testing-library/react';
import { useForm } from 'react-hook-form';
import { AppTextField } from '@shared/components/fields/AppTextField';

const TestForm: React.FC = () => {
  const { control } = useForm({ defaultValues: { name: 'Acme Corp' } });
  return <AppTextField name="name" label="Customer Name" control={control} />;
};

describe('AppTextField', () => {
  it('renders label and initial field value correctly', () => {
    render(<TestForm />);

    expect(screen.getByLabelText(/Customer Name/i)).toBeDefined();
    const input = screen.getByLabelText(/Customer Name/i) as HTMLInputElement;
    expect(input.value).toBe('Acme Corp');
  });
});

# Step 8: Wire Up Client Interactions - Implementation Plan

## Overview

This document provides a comprehensive implementation plan for creating a Next.js 14+ frontend application that interacts with the MythAPI backend. The frontend will implement full CRUD operations for Gods and display Mythologies data.

## Project Context

- **Backend API**: .NET 8.0 ASP.NET Core Minimal API
- **Backend Port**: 5280
- **API Base Path**: `/api/v1`
- **Frontend Framework**: Next.js 14+ with App Router
- **Styling**: Tailwind CSS (recommended)
- **State Management**: React Server Components + Client Components
- **TypeScript**: Strongly typed throughout

## Architecture Overview

```
src/frontend/
├── public/                      # Static assets
├── src/
│   ├── app/                     # Next.js App Router
│   │   ├── layout.tsx          # Root layout
│   │   ├── page.tsx            # Home page
│   │   ├── gods/               # Gods feature
│   │   │   ├── page.tsx        # List all gods
│   │   │   ├── loading.tsx     # Loading state for list
│   │   │   ├── error.tsx       # Error boundary for list
│   │   │   ├── new/
│   │   │   │   ├── page.tsx    # Create new god
│   │   │   │   └── loading.tsx
│   │   │   └── [id]/
│   │   │       ├── page.tsx    # God detail view
│   │   │       ├── loading.tsx
│   │   │       ├── error.tsx
│   │   │       └── edit/
│   │   │           ├── page.tsx    # Edit god
│   │   │           └── loading.tsx
│   │   └── mythologies/        # Mythologies feature
│   │       ├── page.tsx        # List all mythologies
│   │       └── loading.tsx
│   ├── components/             # Reusable components
│   │   ├── gods/
│   │   │   ├── GodCard.tsx     # Display god card (client)
│   │   │   ├── GodForm.tsx     # Create/Edit form (client)
│   │   │   └── GodsList.tsx    # Gods list wrapper
│   │   ├── ui/
│   │   │   ├── SearchBar.tsx   # Search input (client)
│   │   │   ├── Button.tsx      # Reusable button
│   │   │   ├── Card.tsx        # Card container
│   │   │   └── LoadingSpinner.tsx
│   │   └── layout/
│   │       ├── Header.tsx      # App header
│   │       └── Navigation.tsx  # Main navigation
│   ├── lib/                    # Utility functions and services
│   │   ├── api/
│   │   │   ├── gods.ts         # Gods API functions
│   │   │   ├── mythologies.ts  # Mythologies API functions
│   │   │   └── client.ts       # Base API client configuration
│   │   ├── types/
│   │   │   └── index.ts        # TypeScript types/interfaces
│   │   └── utils/
│   │       ├── formatters.ts   # Data formatting utilities
│   │       └── validators.ts   # Input validation
│   └── styles/
│       └── globals.css         # Global styles
├── .env.local                  # Local environment variables
├── .env.example                # Example environment variables
├── next.config.js              # Next.js configuration
├── tailwind.config.ts          # Tailwind configuration
├── tsconfig.json               # TypeScript configuration
├── package.json                # Dependencies
└── README.md                   # Frontend documentation
```

---

## Phase 1: Project Setup and Configuration

### Task 1.1: Initialize Next.js Project

**Location**: `src/frontend/`

**Commands**:
```bash
cd src
npx create-next-app@latest frontend --typescript --tailwind --app --no-src-dir --import-alias "@/*"
cd frontend
```

**Configuration Options**:
- TypeScript: Yes
- ESLint: Yes
- Tailwind CSS: Yes
- App Router: Yes
- Import alias: @/*

**Acceptance Criteria**:
- ✅ Next.js 14+ project initialized
- ✅ TypeScript configured
- ✅ Tailwind CSS configured
- ✅ App Router structure in place

---

### Task 1.2: Configure Environment Variables

**File**: `src/frontend/.env.local`

```env
# API Configuration
NEXT_PUBLIC_API_BASE_URL=http://localhost:5280
NEXT_PUBLIC_API_VERSION=v1

# Feature Flags
NEXT_PUBLIC_ENABLE_SEARCH=true
```

**File**: `src/frontend/.env.example`

```env
# API Configuration
NEXT_PUBLIC_API_BASE_URL=http://localhost:5280
NEXT_PUBLIC_API_VERSION=v1

# Feature Flags
NEXT_PUBLIC_ENABLE_SEARCH=true
```

**Acceptance Criteria**:
- ✅ Environment variables configured
- ✅ Example file created for documentation
- ✅ API URL accessible from components

---

### Task 1.3: Install Additional Dependencies

**File**: `src/frontend/package.json`

**Dependencies to add**:
```bash
npm install axios
npm install react-hot-toast        # For notifications
npm install @heroicons/react       # For icons
npm install clsx                   # For conditional classnames
npm install zod                    # For validation
```

**Dev Dependencies**:
```bash
npm install --save-dev @types/node
```

**Acceptance Criteria**:
- ✅ All dependencies installed
- ✅ Package.json updated
- ✅ No security vulnerabilities

---

### Task 1.4: Configure Next.js

**File**: `src/frontend/next.config.js`

```javascript
/** @type {import('next').NextConfig} */
const nextConfig = {
  // Enable React strict mode for better development warnings
  reactStrictMode: true,
  
  // API rewrites for development (proxy to backend)
  async rewrites() {
    return [
      {
        source: '/api/:path*',
        destination: 'http://localhost:5280/api/:path*', // Proxy to backend
      },
    ];
  },
  
  // Image domains if needed
  images: {
    domains: [],
  },
};

module.exports = nextConfig;
```

**Acceptance Criteria**:
- ✅ API proxy configured for development
- ✅ Strict mode enabled
- ✅ Configuration validated

---

## Phase 2: Type Definitions and API Client

### Task 2.1: Define TypeScript Types

**File**: `src/frontend/src/lib/types/index.ts`

```typescript
/**
 * Core API Types
 * Based on OpenAPI specification
 */

// God Entity
export interface God {
  id: number;
  name: string;
  description: string;
  mythologyId: number;
  aliases: Alias[];
}

// God Input (for create/update)
export interface GodInput {
  id?: number | null;
  name: string;
  description: string;
  mythologyId: number;
}

// Alias Entity
export interface Alias {
  id: number;
  godId: number;
  name: string;
}

// Mythology Entity
export interface Mythology {
  id: number;
  name: string;
  gods: God[];
}

// API Response Types
export interface ApiError {
  message: string;
  statusCode: number;
  timestamp: string;
}

// Search Parameters
export interface SearchGodsParams {
  name: string;
  includeAliases?: boolean;
}

// Form State
export interface GodFormData {
  name: string;
  description: string;
  mythologyId: string; // String for form handling, convert to number
}

// API Client Configuration
export interface ApiClientConfig {
  baseURL: string;
  timeout: number;
}
```

**Acceptance Criteria**:
- ✅ All API entities typed
- ✅ Form data types defined
- ✅ Error types included
- ✅ Types match OpenAPI spec

---

### Task 2.2: Create Base API Client

**File**: `src/frontend/src/lib/api/client.ts`

```typescript
import axios, { AxiosInstance, AxiosError } from 'axios';
import { ApiError } from '../types';

/**
 * Base API client configuration
 */
const API_BASE_URL = process.env.NEXT_PUBLIC_API_BASE_URL || 'http://localhost:5280';
const API_VERSION = process.env.NEXT_PUBLIC_API_VERSION || 'v1';

// Create axios instance with default configuration
export const apiClient: AxiosInstance = axios.create({
  baseURL: `${API_BASE_URL}/api/${API_VERSION}`,
  timeout: 10000,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Request interceptor
apiClient.interceptors.request.use(
  (config) => {
    // Add any auth tokens here if needed
    // config.headers.Authorization = `Bearer ${token}`;
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

// Response interceptor for error handling
apiClient.interceptors.response.use(
  (response) => response,
  (error: AxiosError) => {
    const apiError: ApiError = {
      message: error.response?.data?.toString() || error.message || 'An error occurred',
      statusCode: error.response?.status || 500,
      timestamp: new Date().toISOString(),
    };
    
    return Promise.reject(apiError);
  }
);

/**
 * Helper function to handle API errors
 */
export function handleApiError(error: unknown): string {
  if (isApiError(error)) {
    return error.message;
  }
  
  if (error instanceof Error) {
    return error.message;
  }
  
  return 'An unexpected error occurred';
}

/**
 * Type guard for ApiError
 */
export function isApiError(error: unknown): error is ApiError {
  return (
    typeof error === 'object' &&
    error !== null &&
    'message' in error &&
    'statusCode' in error
  );
}
```

**Key Features**:
- Centralized axios configuration
- Request/response interceptors
- Error handling utilities
- Type-safe error responses

**Acceptance Criteria**:
- ✅ Axios instance configured
- ✅ Base URL from environment variables
- ✅ Error interceptor implemented
- ✅ Type guards for error handling

---

### Task 2.3: Implement Gods API Service

**File**: `src/frontend/src/lib/api/gods.ts`

```typescript
import { apiClient } from './client';
import { God, GodInput, SearchGodsParams } from '../types';

const GODS_ENDPOINT = '/gods';

/**
 * Get all gods
 */
export async function getGods(): Promise<God[]> {
  const response = await apiClient.get<God[]>(GODS_ENDPOINT);
  return response.data;
}

/**
 * Get a single god by ID
 */
export async function getGodById(id: number): Promise<God> {
  const response = await apiClient.get<God>(`${GODS_ENDPOINT}/${id}`);
  return response.data;
}

/**
 * Search gods by name
 */
export async function searchGods({ name, includeAliases = false }: SearchGodsParams): Promise<God[]> {
  const params = new URLSearchParams();
  if (includeAliases) {
    params.append('includeAliases', 'true');
  }
  
  const response = await apiClient.get<God[]>(
    `${GODS_ENDPOINT}/search/${encodeURIComponent(name)}?${params.toString()}`
  );
  return response.data;
}

/**
 * Create a new god
 */
export async function createGod(god: GodInput): Promise<God[]> {
  const response = await apiClient.post<God[]>(GODS_ENDPOINT, [god]);
  return response.data;
}

/**
 * Update an existing god
 */
export async function updateGod(god: GodInput): Promise<God[]> {
  if (!god.id) {
    throw new Error('God ID is required for update operation');
  }
  
  const response = await apiClient.post<God[]>(GODS_ENDPOINT, [god]);
  return response.data;
}

/**
 * Batch create or update gods
 */
export async function batchUpsertGods(gods: GodInput[]): Promise<God[]> {
  if (gods.length === 0) {
    throw new Error('At least one god is required');
  }
  
  if (gods.length > 100) {
    throw new Error('Batch size must not exceed 100 items');
  }
  
  const response = await apiClient.post<God[]>(GODS_ENDPOINT, gods);
  return response.data;
}

/**
 * Delete a god (note: uses batch endpoint)
 * This is a workaround since there's no single delete endpoint
 */
export async function deleteGod(id: number): Promise<void> {
  // Note: The API doesn't have a single delete endpoint
  // This would need to be implemented in the backend
  // For now, throw an error
  throw new Error('Delete operation not supported by API');
}

/**
 * Delete all gods
 */
export async function deleteAllGods(): Promise<void> {
  await apiClient.delete(GODS_ENDPOINT);
}
```

**Key Features**:
- All CRUD operations
- Search functionality
- Batch operations support
- URL encoding for search terms
- Type-safe responses

**Acceptance Criteria**:
- ✅ All API endpoints covered
- ✅ Proper error handling
- ✅ Type safety maintained
- ✅ URL encoding for search

---

### Task 2.4: Implement Mythologies API Service

**File**: `src/frontend/src/lib/api/mythologies.ts`

```typescript
import { apiClient } from './client';
import { Mythology } from '../types';

const MYTHOLOGIES_ENDPOINT = '/mythologies';

/**
 * Get all mythologies with their gods
 */
export async function getMythologies(): Promise<Mythology[]> {
  const response = await apiClient.get<Mythology[]>(MYTHOLOGIES_ENDPOINT);
  return response.data;
}

/**
 * Get mythology options for form select
 * Returns simplified list for dropdown
 */
export async function getMythologyOptions(): Promise<Array<{ id: number; name: string }>> {
  const mythologies = await getMythologies();
  return mythologies.map(({ id, name }) => ({ id, name }));
}
```

**Key Features**:
- Fetch all mythologies
- Helper for form selects
- Type-safe responses

**Acceptance Criteria**:
- ✅ Get mythologies implemented
- ✅ Helper for form options
- ✅ Type safety maintained

---

## Phase 3: Utility Functions and Validators

### Task 3.1: Create Validation Schemas

**File**: `src/frontend/src/lib/utils/validators.ts`

```typescript
import { z } from 'zod';

/**
 * God form validation schema
 */
export const godFormSchema = z.object({
  name: z
    .string()
    .min(1, 'Name is required')
    .max(200, 'Name must not exceed 200 characters')
    .trim(),
  description: z
    .string()
    .min(1, 'Description is required')
    .max(2000, 'Description must not exceed 2000 characters')
    .trim(),
  mythologyId: z
    .string()
    .min(1, 'Mythology is required')
    .transform((val) => parseInt(val, 10))
    .refine((val) => !isNaN(val) && val > 0, 'Invalid mythology selected'),
});

/**
 * Search validation schema
 */
export const searchSchema = z.object({
  query: z
    .string()
    .min(1, 'Search query cannot be empty')
    .max(200, 'Search query must not exceed 200 characters')
    .trim(),
  includeAliases: z.boolean().default(false),
});

/**
 * Validate god form data
 */
export function validateGodForm(data: unknown) {
  return godFormSchema.safeParse(data);
}

/**
 * Validate search input
 */
export function validateSearch(data: unknown) {
  return searchSchema.safeParse(data);
}
```

**Key Features**:
- Zod validation schemas
- Type-safe validation
- Client-side validation
- Proper error messages

**Acceptance Criteria**:
- ✅ Form validation schema
- ✅ Search validation schema
- ✅ Helper functions for validation
- ✅ Error messages defined

---

### Task 3.2: Create Formatting Utilities

**File**: `src/frontend/src/lib/utils/formatters.ts`

```typescript
/**
 * Truncate text to specified length
 */
export function truncate(text: string, maxLength: number): string {
  if (text.length <= maxLength) return text;
  return text.substring(0, maxLength) + '...';
}

/**
 * Format date to readable string
 */
export function formatDate(date: string | Date): string {
  const d = typeof date === 'string' ? new Date(date) : date;
  return d.toLocaleDateString('en-US', {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
  });
}

/**
 * Pluralize word based on count
 */
export function pluralize(count: number, singular: string, plural?: string): string {
  if (count === 1) return singular;
  return plural || `${singular}s`;
}

/**
 * Format god aliases for display
 */
export function formatAliases(aliases: Array<{ name: string }>): string {
  if (aliases.length === 0) return 'No known aliases';
  return aliases.map((a) => a.name).join(', ');
}

/**
 * Get initials from name
 */
export function getInitials(name: string): string {
  return name
    .split(' ')
    .map((word) => word[0])
    .join('')
    .toUpperCase()
    .substring(0, 2);
}
```

**Acceptance Criteria**:
- ✅ Text truncation utility
- ✅ Date formatting
- ✅ Pluralization helper
- ✅ Alias formatting

---

## Phase 4: Reusable UI Components

### Task 4.1: Create Button Component

**File**: `src/frontend/src/components/ui/Button.tsx`

```typescript
import React from 'react';
import clsx from 'clsx';

export interface ButtonProps extends React.ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: 'primary' | 'secondary' | 'danger' | 'ghost';
  size?: 'sm' | 'md' | 'lg';
  isLoading?: boolean;
  fullWidth?: boolean;
}

export function Button({
  children,
  variant = 'primary',
  size = 'md',
  isLoading = false,
  fullWidth = false,
  disabled,
  className,
  ...props
}: ButtonProps) {
  const baseStyles = 'inline-flex items-center justify-center font-medium rounded-lg transition-colors focus:outline-none focus:ring-2 focus:ring-offset-2 disabled:opacity-50 disabled:cursor-not-allowed';
  
  const variants = {
    primary: 'bg-blue-600 text-white hover:bg-blue-700 focus:ring-blue-500',
    secondary: 'bg-gray-200 text-gray-900 hover:bg-gray-300 focus:ring-gray-500',
    danger: 'bg-red-600 text-white hover:bg-red-700 focus:ring-red-500',
    ghost: 'bg-transparent text-gray-700 hover:bg-gray-100 focus:ring-gray-500',
  };
  
  const sizes = {
    sm: 'px-3 py-1.5 text-sm',
    md: 'px-4 py-2 text-base',
    lg: 'px-6 py-3 text-lg',
  };
  
  return (
    <button
      className={clsx(
        baseStyles,
        variants[variant],
        sizes[size],
        fullWidth && 'w-full',
        className
      )}
      disabled={disabled || isLoading}
      {...props}
    >
      {isLoading && (
        <svg
          className="animate-spin -ml-1 mr-2 h-4 w-4"
          xmlns="http://www.w3.org/2000/svg"
          fill="none"
          viewBox="0 0 24 24"
        >
          <circle
            className="opacity-25"
            cx="12"
            cy="12"
            r="10"
            stroke="currentColor"
            strokeWidth="4"
          />
          <path
            className="opacity-75"
            fill="currentColor"
            d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"
          />
        </svg>
      )}
      {children}
    </button>
  );
}
```

**Acceptance Criteria**:
- ✅ Multiple variants (primary, secondary, danger, ghost)
- ✅ Multiple sizes
- ✅ Loading state
- ✅ Full width option
- ✅ Accessible

---

### Task 4.2: Create Card Component

**File**: `src/frontend/src/components/ui/Card.tsx`

```typescript
import React from 'react';
import clsx from 'clsx';

export interface CardProps {
  children: React.ReactNode;
  className?: string;
  onClick?: () => void;
  hover?: boolean;
}

export function Card({ children, className, onClick, hover = false }: CardProps) {
  return (
    <div
      className={clsx(
        'bg-white rounded-lg shadow-md border border-gray-200 overflow-hidden',
        hover && 'transition-shadow hover:shadow-lg',
        onClick && 'cursor-pointer',
        className
      )}
      onClick={onClick}
    >
      {children}
    </div>
  );
}

export interface CardHeaderProps {
  children: React.ReactNode;
  className?: string;
}

export function CardHeader({ children, className }: CardHeaderProps) {
  return (
    <div className={clsx('px-6 py-4 border-b border-gray-200', className)}>
      {children}
    </div>
  );
}

export interface CardBodyProps {
  children: React.ReactNode;
  className?: string;
}

export function CardBody({ children, className }: CardBodyProps) {
  return <div className={clsx('px-6 py-4', className)}>{children}</div>;
}

export interface CardFooterProps {
  children: React.ReactNode;
  className?: string;
}

export function CardFooter({ children, className }: CardFooterProps) {
  return (
    <div className={clsx('px-6 py-4 bg-gray-50 border-t border-gray-200', className)}>
      {children}
    </div>
  );
}
```

**Acceptance Criteria**:
- ✅ Card container component
- ✅ Card header, body, footer sub-components
- ✅ Hover effects
- ✅ Clickable option

---

### Task 4.3: Create Loading Spinner

**File**: `src/frontend/src/components/ui/LoadingSpinner.tsx`

```typescript
import React from 'react';
import clsx from 'clsx';

export interface LoadingSpinnerProps {
  size?: 'sm' | 'md' | 'lg';
  className?: string;
}

export function LoadingSpinner({ size = 'md', className }: LoadingSpinnerProps) {
  const sizes = {
    sm: 'h-4 w-4',
    md: 'h-8 w-8',
    lg: 'h-12 w-12',
  };
  
  return (
    <div className={clsx('flex items-center justify-center', className)}>
      <svg
        className={clsx('animate-spin text-blue-600', sizes[size])}
        xmlns="http://www.w3.org/2000/svg"
        fill="none"
        viewBox="0 0 24 24"
      >
        <circle
          className="opacity-25"
          cx="12"
          cy="12"
          r="10"
          stroke="currentColor"
          strokeWidth="4"
        />
        <path
          className="opacity-75"
          fill="currentColor"
          d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"
        />
      </svg>
    </div>
  );
}

export function LoadingPage() {
  return (
    <div className="flex items-center justify-center min-h-[400px]">
      <div className="text-center">
        <LoadingSpinner size="lg" />
        <p className="mt-4 text-gray-600">Loading...</p>
      </div>
    </div>
  );
}
```

**Acceptance Criteria**:
- ✅ Spinner component with sizes
- ✅ Full page loading component
- ✅ Animated spinner

---

### Task 4.4: Create SearchBar Component (Client)

**File**: `src/frontend/src/components/ui/SearchBar.tsx`

```typescript
'use client';

import React, { useState, useCallback } from 'react';
import { MagnifyingGlassIcon, XMarkIcon } from '@heroicons/react/24/outline';
import clsx from 'clsx';
import { debounce } from 'lodash';

export interface SearchBarProps {
  onSearch: (query: string) => void;
  placeholder?: string;
  debounceMs?: number;
  className?: string;
}

export function SearchBar({
  onSearch,
  placeholder = 'Search...',
  debounceMs = 300,
  className,
}: SearchBarProps) {
  const [query, setQuery] = useState('');
  
  // Debounce search to avoid too many API calls
  const debouncedSearch = useCallback(
    debounce((searchQuery: string) => {
      onSearch(searchQuery);
    }, debounceMs),
    [onSearch, debounceMs]
  );
  
  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const value = e.target.value;
    setQuery(value);
    debouncedSearch(value);
  };
  
  const handleClear = () => {
    setQuery('');
    onSearch('');
  };
  
  return (
    <div className={clsx('relative', className)}>
      <div className="relative">
        <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
          <MagnifyingGlassIcon className="h-5 w-5 text-gray-400" />
        </div>
        <input
          type="text"
          value={query}
          onChange={handleChange}
          className="block w-full pl-10 pr-10 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-blue-500 focus:border-transparent"
          placeholder={placeholder}
        />
        {query && (
          <button
            onClick={handleClear}
            className="absolute inset-y-0 right-0 pr-3 flex items-center"
            aria-label="Clear search"
          >
            <XMarkIcon className="h-5 w-5 text-gray-400 hover:text-gray-600" />
          </button>
        )}
      </div>
    </div>
  );
}
```

**Key Features**:
- Client component with state
- Debounced search
- Clear button
- Icon integration
- Accessible

**Acceptance Criteria**:
- ✅ Client component directive
- ✅ Debounced search input
- ✅ Clear functionality
- ✅ Icons from Heroicons
- ✅ Keyboard accessible

---

## Phase 5: God-Specific Components

### Task 5.1: Create GodCard Component (Client)

**File**: `src/frontend/src/components/gods/GodCard.tsx`

```typescript
'use client';

import React from 'react';
import Link from 'next/link';
import { Card, CardBody, CardFooter } from '@/components/ui/Card';
import { Button } from '@/components/ui/Button';
import { God } from '@/lib/types';
import { truncate, formatAliases } from '@/lib/utils/formatters';

export interface GodCardProps {
  god: God;
  onEdit?: (id: number) => void;
  onDelete?: (id: number) => void;
}

export function GodCard({ god, onEdit, onDelete }: GodCardProps) {
  return (
    <Card hover>
      <CardBody>
        <Link href={`/gods/${god.id}`}>
          <h3 className="text-xl font-bold text-gray-900 hover:text-blue-600 transition-colors">
            {god.name}
          </h3>
        </Link>
        
        <p className="mt-2 text-gray-600 text-sm">
          {truncate(god.description, 150)}
        </p>
        
        {god.aliases && god.aliases.length > 0 && (
          <div className="mt-3">
            <p className="text-xs text-gray-500">
              <span className="font-semibold">Also known as:</span>{' '}
              {formatAliases(god.aliases)}
            </p>
          </div>
        )}
        
        <div className="mt-3">
          <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-blue-100 text-blue-800">
            Mythology ID: {god.mythologyId}
          </span>
        </div>
      </CardBody>
      
      {(onEdit || onDelete) && (
        <CardFooter>
          <div className="flex gap-2">
            {onEdit && (
              <Button
                variant="secondary"
                size="sm"
                onClick={() => onEdit(god.id)}
              >
                Edit
              </Button>
            )}
            {onDelete && (
              <Button
                variant="danger"
                size="sm"
                onClick={() => onDelete(god.id)}
              >
                Delete
              </Button>
            )}
          </div>
        </CardFooter>
      )}
    </Card>
  );
}
```

**Key Features**:
- Client component for interactivity
- Link to detail page
- Truncated description
- Aliases display
- Edit/Delete actions
- Responsive design

**Acceptance Criteria**:
- ✅ Client component directive
- ✅ Links to detail page
- ✅ Conditional action buttons
- ✅ Proper styling
- ✅ Uses shared UI components

---

### Task 5.2: Create GodForm Component (Client)

**File**: `src/frontend/src/components/gods/GodForm.tsx`

```typescript
'use client';

import React, { useState, useEffect } from 'react';
import { useRouter } from 'next/navigation';
import toast from 'react-hot-toast';
import { Button } from '@/components/ui/Button';
import { God, GodInput, GodFormData } from '@/lib/types';
import { validateGodForm } from '@/lib/utils/validators';
import { createGod, updateGod } from '@/lib/api/gods';
import { getMythologyOptions } from '@/lib/api/mythologies';

export interface GodFormProps {
  god?: God; // If provided, form is in edit mode
  onSuccess?: () => void;
  onCancel?: () => void;
}

export function GodForm({ god, onSuccess, onCancel }: GodFormProps) {
  const router = useRouter();
  const isEditMode = !!god;
  
  const [formData, setFormData] = useState<GodFormData>({
    name: god?.name || '',
    description: god?.description || '',
    mythologyId: god?.mythologyId?.toString() || '',
  });
  
  const [mythologies, setMythologies] = useState<Array<{ id: number; name: string }>>([]);
  const [errors, setErrors] = useState<Partial<Record<keyof GodFormData, string>>>({});
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [isLoadingMythologies, setIsLoadingMythologies] = useState(true);
  
  // Load mythologies on mount
  useEffect(() => {
    async function loadMythologies() {
      try {
        const data = await getMythologyOptions();
        setMythologies(data);
      } catch (error) {
        toast.error('Failed to load mythologies');
        console.error('Error loading mythologies:', error);
      } finally {
        setIsLoadingMythologies(false);
      }
    }
    
    loadMythologies();
  }, []);
  
  const handleChange = (
    e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>
  ) => {
    const { name, value } = e.target;
    setFormData((prev) => ({ ...prev, [name]: value }));
    
    // Clear error for this field
    if (errors[name as keyof GodFormData]) {
      setErrors((prev) => ({ ...prev, [name]: undefined }));
    }
  };
  
  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    // Validate form
    const validation = validateGodForm(formData);
    
    if (!validation.success) {
      const fieldErrors: Partial<Record<keyof GodFormData, string>> = {};
      validation.error.errors.forEach((err) => {
        const field = err.path[0] as keyof GodFormData;
        fieldErrors[field] = err.message;
      });
      setErrors(fieldErrors);
      return;
    }
    
    setIsSubmitting(true);
    
    try {
      const godInput: GodInput = {
        ...(isEditMode && { id: god.id }),
        name: validation.data.name,
        description: validation.data.description,
        mythologyId: validation.data.mythologyId,
      };
      
      if (isEditMode) {
        await updateGod(godInput);
        toast.success('God updated successfully!');
      } else {
        await createGod(godInput);
        toast.success('God created successfully!');
      }
      
      if (onSuccess) {
        onSuccess();
      } else {
        router.push('/gods');
        router.refresh();
      }
    } catch (error) {
      console.error('Error submitting form:', error);
      toast.error(isEditMode ? 'Failed to update god' : 'Failed to create god');
    } finally {
      setIsSubmitting(false);
    }
  };
  
  if (isLoadingMythologies) {
    return (
      <div className="flex items-center justify-center p-8">
        <div className="text-gray-600">Loading form...</div>
      </div>
    );
  }
  
  return (
    <form onSubmit={handleSubmit} className="space-y-6">
      {/* Name Field */}
      <div>
        <label htmlFor="name" className="block text-sm font-medium text-gray-700">
          Name *
        </label>
        <input
          type="text"
          id="name"
          name="name"
          value={formData.name}
          onChange={handleChange}
          className={`mt-1 block w-full rounded-md border ${
            errors.name ? 'border-red-500' : 'border-gray-300'
          } px-3 py-2 focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500`}
          placeholder="e.g., Zeus"
        />
        {errors.name && (
          <p className="mt-1 text-sm text-red-600">{errors.name}</p>
        )}
      </div>
      
      {/* Description Field */}
      <div>
        <label htmlFor="description" className="block text-sm font-medium text-gray-700">
          Description *
        </label>
        <textarea
          id="description"
          name="description"
          value={formData.description}
          onChange={handleChange}
          rows={5}
          className={`mt-1 block w-full rounded-md border ${
            errors.description ? 'border-red-500' : 'border-gray-300'
          } px-3 py-2 focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500`}
          placeholder="Describe the god's role, powers, and significance..."
        />
        {errors.description && (
          <p className="mt-1 text-sm text-red-600">{errors.description}</p>
        )}
      </div>
      
      {/* Mythology Field */}
      <div>
        <label htmlFor="mythologyId" className="block text-sm font-medium text-gray-700">
          Mythology *
        </label>
        <select
          id="mythologyId"
          name="mythologyId"
          value={formData.mythologyId}
          onChange={handleChange}
          className={`mt-1 block w-full rounded-md border ${
            errors.mythologyId ? 'border-red-500' : 'border-gray-300'
          } px-3 py-2 focus:border-blue-500 focus:outline-none focus:ring-1 focus:ring-blue-500`}
        >
          <option value="">Select a mythology</option>
          {mythologies.map((mythology) => (
            <option key={mythology.id} value={mythology.id}>
              {mythology.name}
            </option>
          ))}
        </select>
        {errors.mythologyId && (
          <p className="mt-1 text-sm text-red-600">{errors.mythologyId}</p>
        )}
      </div>
      
      {/* Form Actions */}
      <div className="flex gap-3">
        <Button
          type="submit"
          variant="primary"
          isLoading={isSubmitting}
          disabled={isSubmitting}
        >
          {isEditMode ? 'Update God' : 'Create God'}
        </Button>
        
        {onCancel && (
          <Button
            type="button"
            variant="secondary"
            onClick={onCancel}
            disabled={isSubmitting}
          >
            Cancel
          </Button>
        )}
      </div>
    </form>
  );
}
```

**Key Features**:
- Client component with form state
- Validation with Zod
- Error display per field
- Loading states
- Edit/Create modes
- Toast notifications
- Router integration

**Acceptance Criteria**:
- ✅ Client component directive
- ✅ Form validation
- ✅ Error messages
- ✅ Loading states
- ✅ Edit and create modes
- ✅ Mythology dropdown
- ✅ Toast notifications

---

### Task 5.3: Create GodsList Component

**File**: `src/frontend/src/components/gods/GodsList.tsx`

```typescript
import React from 'react';
import { GodCard } from './GodCard';
import { God } from '@/lib/types';

export interface GodsListProps {
  gods: God[];
  emptyMessage?: string;
}

export function GodsList({ gods, emptyMessage = 'No gods found' }: GodsListProps) {
  if (gods.length === 0) {
    return (
      <div className="text-center py-12">
        <p className="text-gray-500 text-lg">{emptyMessage}</p>
      </div>
    );
  }
  
  return (
    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
      {gods.map((god) => (
        <GodCard key={god.id} god={god} />
      ))}
    </div>
  );
}
```

**Acceptance Criteria**:
- ✅ Grid layout
- ✅ Empty state
- ✅ Responsive design
- ✅ Uses GodCard component

---

## Phase 6: Layout Components

### Task 6.1: Create Header Component

**File**: `src/frontend/src/components/layout/Header.tsx`

```typescript
import React from 'react';
import Link from 'next/link';

export function Header() {
  return (
    <header className="bg-white shadow-sm border-b border-gray-200">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div className="flex justify-between items-center h-16">
          <div className="flex items-center">
            <Link href="/" className="flex items-center">
              <span className="text-2xl font-bold text-blue-600">MythAPI</span>
            </Link>
          </div>
          
          <nav className="flex space-x-8">
            <Link
              href="/gods"
              className="text-gray-700 hover:text-blue-600 px-3 py-2 text-sm font-medium transition-colors"
            >
              Gods
            </Link>
            <Link
              href="/mythologies"
              className="text-gray-700 hover:text-blue-600 px-3 py-2 text-sm font-medium transition-colors"
            >
              Mythologies
            </Link>
          </nav>
        </div>
      </div>
    </header>
  );
}
```

**Acceptance Criteria**:
- ✅ Logo/branding
- ✅ Navigation links
- ✅ Responsive design
- ✅ Active link styling

---

### Task 6.2: Create Footer Component

**File**: `src/frontend/src/components/layout/Footer.tsx`

```typescript
import React from 'react';

export function Footer() {
  const currentYear = new Date().getFullYear();
  
  return (
    <footer className="bg-gray-50 border-t border-gray-200 mt-auto">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <div className="text-center text-gray-600 text-sm">
          <p>&copy; {currentYear} MythAPI. Built with Next.js and .NET 8.0</p>
          <p className="mt-2">
            <a
              href="https://github.com/SolidifyDemo/mythapi-demo"
              target="_blank"
              rel="noopener noreferrer"
              className="text-blue-600 hover:text-blue-700"
            >
              View on GitHub
            </a>
          </p>
        </div>
      </div>
    </footer>
  );
}
```

**Acceptance Criteria**:
- ✅ Copyright notice
- ✅ Links
- ✅ Responsive design

---

## Phase 7: App Router Pages

### Task 7.1: Create Root Layout

**File**: `src/frontend/src/app/layout.tsx`

```typescript
import type { Metadata } from 'next';
import { Inter } from 'next/font/google';
import { Toaster } from 'react-hot-toast';
import { Header } from '@/components/layout/Header';
import { Footer } from '@/components/layout/Footer';
import './globals.css';

const inter = Inter({ subsets: ['latin'] });

export const metadata: Metadata = {
  title: 'MythAPI - Explore Gods and Mythologies',
  description: 'A comprehensive database of gods and mythologies from around the world',
};

export default function RootLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <html lang="en">
      <body className={inter.className}>
        <div className="flex flex-col min-h-screen">
          <Header />
          <main className="flex-1 bg-gray-50">
            {children}
          </main>
          <Footer />
        </div>
        <Toaster position="top-right" />
      </body>
    </html>
  );
}
```

**Acceptance Criteria**:
- ✅ Root layout structure
- ✅ Header and Footer included
- ✅ Toast notifications
- ✅ Metadata defined
- ✅ Font configuration

---

### Task 7.2: Create Home Page

**File**: `src/frontend/src/app/page.tsx`

```typescript
import React from 'react';
import Link from 'next/link';
import { Button } from '@/components/ui/Button';

export default function HomePage() {
  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-12">
      <div className="text-center">
        <h1 className="text-4xl font-bold text-gray-900 sm:text-5xl md:text-6xl">
          Welcome to <span className="text-blue-600">MythAPI</span>
        </h1>
        
        <p className="mt-6 text-xl text-gray-600 max-w-3xl mx-auto">
          Explore a comprehensive database of gods and mythologies from cultures around the world.
          Search, discover, and learn about ancient deities and their stories.
        </p>
        
        <div className="mt-10 flex gap-4 justify-center">
          <Link href="/gods">
            <Button variant="primary" size="lg">
              Browse Gods
            </Button>
          </Link>
          <Link href="/mythologies">
            <Button variant="secondary" size="lg">
              Explore Mythologies
            </Button>
          </Link>
        </div>
        
        <div className="mt-20 grid grid-cols-1 gap-8 sm:grid-cols-3">
          <div className="bg-white p-6 rounded-lg shadow-md">
            <div className="text-3xl font-bold text-blue-600 mb-2">Search</div>
            <p className="text-gray-600">
              Find gods by name or alias with our powerful search feature
            </p>
          </div>
          
          <div className="bg-white p-6 rounded-lg shadow-md">
            <div className="text-3xl font-bold text-blue-600 mb-2">Discover</div>
            <p className="text-gray-600">
              Learn about gods from Greek, Norse, Egyptian, and many more mythologies
            </p>
          </div>
          
          <div className="bg-white p-6 rounded-lg shadow-md">
            <div className="text-3xl font-bold text-blue-600 mb-2">Contribute</div>
            <p className="text-gray-600">
              Add new gods and update existing information to grow our database
            </p>
          </div>
        </div>
      </div>
    </div>
  );
}
```

**Acceptance Criteria**:
- ✅ Hero section
- ✅ Call-to-action buttons
- ✅ Feature highlights
- ✅ Responsive design

---

### Task 7.3: Create Gods List Page

**File**: `src/frontend/src/app/gods/page.tsx`

```typescript
'use client';

import React, { useState, useEffect } from 'react';
import Link from 'next/link';
import { useRouter } from 'next/navigation';
import toast from 'react-hot-toast';
import { Button } from '@/components/ui/Button';
import { SearchBar } from '@/components/ui/SearchBar';
import { LoadingPage } from '@/components/ui/LoadingSpinner';
import { GodCard } from '@/components/gods/GodCard';
import { God } from '@/lib/types';
import { getGods, searchGods } from '@/lib/api/gods';
import { PlusIcon } from '@heroicons/react/24/outline';

export default function GodsPage() {
  const router = useRouter();
  const [gods, setGods] = useState<God[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [searchQuery, setSearchQuery] = useState('');
  
  useEffect(() => {
    loadGods();
  }, []);
  
  async function loadGods() {
    try {
      setIsLoading(true);
      const data = await getGods();
      setGods(data);
    } catch (error) {
      console.error('Error loading gods:', error);
      toast.error('Failed to load gods');
    } finally {
      setIsLoading(false);
    }
  }
  
  async function handleSearch(query: string) {
    setSearchQuery(query);
    
    if (!query.trim()) {
      loadGods();
      return;
    }
    
    try {
      const results = await searchGods({ name: query, includeAliases: true });
      setGods(results);
    } catch (error) {
      console.error('Error searching gods:', error);
      toast.error('Search failed');
    }
  }
  
  function handleEdit(id: number) {
    router.push(`/gods/${id}/edit`);
  }
  
  if (isLoading) {
    return <LoadingPage />;
  }
  
  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      {/* Header */}
      <div className="flex justify-between items-center mb-8">
        <div>
          <h1 className="text-3xl font-bold text-gray-900">Gods</h1>
          <p className="mt-2 text-gray-600">
            Browse and search through our collection of {gods.length} gods
          </p>
        </div>
        <Link href="/gods/new">
          <Button variant="primary">
            <PlusIcon className="h-5 w-5 mr-2" />
            Add God
          </Button>
        </Link>
      </div>
      
      {/* Search Bar */}
      <div className="mb-8">
        <SearchBar
          onSearch={handleSearch}
          placeholder="Search gods by name or alias..."
          className="max-w-2xl"
        />
      </div>
      
      {/* Gods Grid */}
      {gods.length === 0 ? (
        <div className="text-center py-12">
          <p className="text-gray-500 text-lg">
            {searchQuery ? 'No gods found matching your search' : 'No gods available'}
          </p>
        </div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {gods.map((god) => (
            <GodCard key={god.id} god={god} onEdit={handleEdit} />
          ))}
        </div>
      )}
    </div>
  );
}
```

**Key Features**:
- Client component for interactivity
- Search integration
- Loading states
- Empty states
- Grid layout
- Add god button

**Acceptance Criteria**:
- ✅ Displays all gods
- ✅ Search functionality
- ✅ Loading state
- ✅ Empty state
- ✅ Link to create page
- ✅ Edit functionality

---

### Task 7.4: Create Gods Loading State

**File**: `src/frontend/src/app/gods/loading.tsx`

```typescript
import { LoadingPage } from '@/components/ui/LoadingSpinner';

export default function GodsLoading() {
  return <LoadingPage />;
}
```

**Acceptance Criteria**:
- ✅ Loading UI displayed while fetching

---

### Task 7.5: Create Gods Error State

**File**: `src/frontend/src/app/gods/error.tsx`

```typescript
'use client';

import React from 'react';
import { Button } from '@/components/ui/Button';

export default function GodsError({
  error,
  reset,
}: {
  error: Error & { digest?: string };
  reset: () => void;
}) {
  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-12">
      <div className="text-center">
        <h2 className="text-2xl font-bold text-gray-900 mb-4">
          Something went wrong!
        </h2>
        <p className="text-gray-600 mb-8">
          {error.message || 'Failed to load gods'}
        </p>
        <Button onClick={reset} variant="primary">
          Try Again
        </Button>
      </div>
    </div>
  );
}
```

**Acceptance Criteria**:
- ✅ Error boundary
- ✅ Error message display
- ✅ Retry button

---

### Task 7.6: Create God Detail Page

**File**: `src/frontend/src/app/gods/[id]/page.tsx`

```typescript
import React from 'react';
import Link from 'next/link';
import { notFound } from 'next/navigation';
import { Button } from '@/components/ui/Button';
import { Card, CardBody } from '@/components/ui/Card';
import { getGodById } from '@/lib/api/gods';
import { formatAliases } from '@/lib/utils/formatters';
import { PencilIcon, ArrowLeftIcon } from '@heroicons/react/24/outline';

interface GodDetailPageProps {
  params: {
    id: string;
  };
}

export default async function GodDetailPage({ params }: GodDetailPageProps) {
  const godId = parseInt(params.id, 10);
  
  if (isNaN(godId) || godId <= 0) {
    notFound();
  }
  
  let god;
  try {
    god = await getGodById(godId);
  } catch (error) {
    notFound();
  }
  
  return (
    <div className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      {/* Back Button */}
      <Link href="/gods" className="inline-flex items-center text-blue-600 hover:text-blue-700 mb-6">
        <ArrowLeftIcon className="h-5 w-5 mr-2" />
        Back to Gods
      </Link>
      
      {/* God Details */}
      <div className="mb-6">
        <div className="flex justify-between items-start">
          <h1 className="text-4xl font-bold text-gray-900">{god.name}</h1>
          <Link href={`/gods/${god.id}/edit`}>
            <Button variant="secondary">
              <PencilIcon className="h-5 w-5 mr-2" />
              Edit
            </Button>
          </Link>
        </div>
        
        <div className="mt-4">
          <span className="inline-flex items-center px-3 py-1 rounded-full text-sm font-medium bg-blue-100 text-blue-800">
            Mythology ID: {god.mythologyId}
          </span>
        </div>
      </div>
      
      {/* Description */}
      <Card className="mb-6">
        <CardBody>
          <h2 className="text-xl font-semibold text-gray-900 mb-3">Description</h2>
          <p className="text-gray-700 leading-relaxed">{god.description}</p>
        </CardBody>
      </Card>
      
      {/* Aliases */}
      {god.aliases && god.aliases.length > 0 && (
        <Card>
          <CardBody>
            <h2 className="text-xl font-semibold text-gray-900 mb-3">Known As</h2>
            <div className="flex flex-wrap gap-2">
              {god.aliases.map((alias) => (
                <span
                  key={alias.id}
                  className="inline-flex items-center px-3 py-1 rounded-full text-sm font-medium bg-gray-100 text-gray-800"
                >
                  {alias.name}
                </span>
              ))}
            </div>
          </CardBody>
        </Card>
      )}
      
      {/* Metadata */}
      <div className="mt-8 text-sm text-gray-500">
        <p>God ID: {god.id}</p>
      </div>
    </div>
  );
}
```

**Key Features**:
- Server component (default)
- Dynamic route parameter
- 404 handling
- Back navigation
- Edit button
- Detailed information display

**Acceptance Criteria**:
- ✅ Dynamic route with [id]
- ✅ Fetches god by ID
- ✅ Displays all god information
- ✅ Back button
- ✅ Edit button
- ✅ 404 for invalid IDs
- ✅ Aliases display

---

### Task 7.7: Create God Detail Loading State

**File**: `src/frontend/src/app/gods/[id]/loading.tsx`

```typescript
import { LoadingPage } from '@/components/ui/LoadingSpinner';

export default function GodDetailLoading() {
  return <LoadingPage />;
}
```

---

### Task 7.8: Create God Detail Error State

**File**: `src/frontend/src/app/gods/[id]/error.tsx`

```typescript
'use client';

import React from 'react';
import Link from 'next/link';
import { Button } from '@/components/ui/Button';

export default function GodDetailError({
  error,
  reset,
}: {
  error: Error & { digest?: string };
  reset: () => void;
}) {
  return (
    <div className="max-w-4xl mx-auto px-4 sm:px-6 lg:px-8 py-12">
      <div className="text-center">
        <h2 className="text-2xl font-bold text-gray-900 mb-4">
          Failed to Load God
        </h2>
        <p className="text-gray-600 mb-8">
          {error.message || 'The god you are looking for could not be loaded'}
        </p>
        <div className="flex gap-4 justify-center">
          <Button onClick={reset} variant="primary">
            Try Again
          </Button>
          <Link href="/gods">
            <Button variant="secondary">
              Back to Gods
            </Button>
          </Link>
        </div>
      </div>
    </div>
  );
}
```

---

### Task 7.9: Create New God Page

**File**: `src/frontend/src/app/gods/new/page.tsx`

```typescript
import React from 'react';
import Link from 'next/link';
import { Card, CardBody, CardHeader } from '@/components/ui/Card';
import { GodForm } from '@/components/gods/GodForm';
import { ArrowLeftIcon } from '@heroicons/react/24/outline';

export default function NewGodPage() {
  return (
    <div className="max-w-3xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <Link href="/gods" className="inline-flex items-center text-blue-600 hover:text-blue-700 mb-6">
        <ArrowLeftIcon className="h-5 w-5 mr-2" />
        Back to Gods
      </Link>
      
      <Card>
        <CardHeader>
          <h1 className="text-2xl font-bold text-gray-900">Create New God</h1>
          <p className="mt-1 text-sm text-gray-600">
            Add a new god to the mythology database
          </p>
        </CardHeader>
        <CardBody>
          <GodForm />
        </CardBody>
      </Card>
    </div>
  );
}
```

**Acceptance Criteria**:
- ✅ Form for creating new god
- ✅ Back navigation
- ✅ Uses GodForm component
- ✅ Proper page layout

---

### Task 7.10: Create New God Loading State

**File**: `src/frontend/src/app/gods/new/loading.tsx`

```typescript
import { LoadingPage } from '@/components/ui/LoadingSpinner';

export default function NewGodLoading() {
  return <LoadingPage />;
}
```

---

### Task 7.11: Create Edit God Page

**File**: `src/frontend/src/app/gods/[id]/edit/page.tsx`

```typescript
import React from 'react';
import Link from 'next/link';
import { notFound } from 'next/navigation';
import { Card, CardBody, CardHeader } from '@/components/ui/Card';
import { GodForm } from '@/components/gods/GodForm';
import { getGodById } from '@/lib/api/gods';
import { ArrowLeftIcon } from '@heroicons/react/24/outline';

interface EditGodPageProps {
  params: {
    id: string;
  };
}

export default async function EditGodPage({ params }: EditGodPageProps) {
  const godId = parseInt(params.id, 10);
  
  if (isNaN(godId) || godId <= 0) {
    notFound();
  }
  
  let god;
  try {
    god = await getGodById(godId);
  } catch (error) {
    notFound();
  }
  
  return (
    <div className="max-w-3xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <Link href={`/gods/${god.id}`} className="inline-flex items-center text-blue-600 hover:text-blue-700 mb-6">
        <ArrowLeftIcon className="h-5 w-5 mr-2" />
        Back to {god.name}
      </Link>
      
      <Card>
        <CardHeader>
          <h1 className="text-2xl font-bold text-gray-900">Edit God</h1>
          <p className="mt-1 text-sm text-gray-600">
            Update information for {god.name}
          </p>
        </CardHeader>
        <CardBody>
          <GodForm god={god} />
        </CardBody>
      </Card>
    </div>
  );
}
```

**Acceptance Criteria**:
- ✅ Edit form with pre-filled data
- ✅ Back navigation
- ✅ Uses GodForm component in edit mode
- ✅ 404 for invalid IDs

---

### Task 7.12: Create Edit God Loading State

**File**: `src/frontend/src/app/gods/[id]/edit/loading.tsx`

```typescript
import { LoadingPage } from '@/components/ui/LoadingSpinner';

export default function EditGodLoading() {
  return <LoadingPage />;
}
```

---

### Task 7.13: Create Mythologies Page

**File**: `src/frontend/src/app/mythologies/page.tsx`

```typescript
import React from 'react';
import { Card, CardBody, CardHeader } from '@/components/ui/Card';
import { getMythologies } from '@/lib/api/mythologies';
import { pluralize } from '@/lib/utils/formatters';

export default async function MythologiesPage() {
  const mythologies = await getMythologies();
  
  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <div className="mb-8">
        <h1 className="text-3xl font-bold text-gray-900">Mythologies</h1>
        <p className="mt-2 text-gray-600">
          Explore {mythologies.length} {pluralize(mythologies.length, 'mythology', 'mythologies')}
        </p>
      </div>
      
      {mythologies.length === 0 ? (
        <div className="text-center py-12">
          <p className="text-gray-500 text-lg">No mythologies available</p>
        </div>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {mythologies.map((mythology) => (
            <Card key={mythology.id} hover>
              <CardHeader>
                <h2 className="text-xl font-bold text-gray-900">{mythology.name}</h2>
              </CardHeader>
              <CardBody>
                <div className="space-y-3">
                  <div>
                    <p className="text-sm text-gray-600">
                      {mythology.gods.length} {pluralize(mythology.gods.length, 'god')}
                    </p>
                  </div>
                  
                  {mythology.gods.length > 0 && (
                    <div>
                      <p className="text-xs font-semibold text-gray-700 mb-2">Featured Gods:</p>
                      <div className="flex flex-wrap gap-2">
                        {mythology.gods.slice(0, 5).map((god) => (
                          <span
                            key={god.id}
                            className="inline-flex items-center px-2 py-1 rounded text-xs font-medium bg-blue-100 text-blue-800"
                          >
                            {god.name}
                          </span>
                        ))}
                        {mythology.gods.length > 5 && (
                          <span className="inline-flex items-center px-2 py-1 rounded text-xs font-medium bg-gray-100 text-gray-600">
                            +{mythology.gods.length - 5} more
                          </span>
                        )}
                      </div>
                    </div>
                  )}
                </div>
              </CardBody>
            </Card>
          ))}
        </div>
      )}
    </div>
  );
}
```

**Acceptance Criteria**:
- ✅ Displays all mythologies
- ✅ Shows god count per mythology
- ✅ Featured gods preview
- ✅ Grid layout
- ✅ Empty state

---

### Task 7.14: Create Mythologies Loading State

**File**: `src/frontend/src/app/mythologies/loading.tsx`

```typescript
import { LoadingPage } from '@/components/ui/LoadingSpinner';

export default function MythologiesLoading() {
  return <LoadingPage />;
}
```

---

### Task 7.15: Update Global Styles

**File**: `src/frontend/src/app/globals.css`

```css
@tailwind base;
@tailwind components;
@tailwind utilities;

@layer base {
  html {
    @apply scroll-smooth;
  }
  
  body {
    @apply text-gray-900 antialiased;
  }
}

@layer utilities {
  .text-balance {
    text-wrap: balance;
  }
}
```

---

## Phase 8: Configuration and Documentation

### Task 8.1: Update Tailwind Configuration

**File**: `src/frontend/tailwind.config.ts`

```typescript
import type { Config } from 'tailwindcss';

const config: Config = {
  content: [
    './src/pages/**/*.{js,ts,jsx,tsx,mdx}',
    './src/components/**/*.{js,ts,jsx,tsx,mdx}',
    './src/app/**/*.{js,ts,jsx,tsx,mdx}',
  ],
  theme: {
    extend: {
      colors: {
        primary: {
          50: '#eff6ff',
          100: '#dbeafe',
          200: '#bfdbfe',
          300: '#93c5fd',
          400: '#60a5fa',
          500: '#3b82f6',
          600: '#2563eb',
          700: '#1d4ed8',
          800: '#1e40af',
          900: '#1e3a8a',
        },
      },
      animation: {
        'spin-slow': 'spin 3s linear infinite',
      },
    },
  },
  plugins: [],
};

export default config;
```

---

### Task 8.2: Update TypeScript Configuration

**File**: `src/frontend/tsconfig.json`

```json
{
  "compilerOptions": {
    "target": "ES2017",
    "lib": ["dom", "dom.iterable", "esnext"],
    "allowJs": true,
    "skipLibCheck": true,
    "strict": true,
    "noEmit": true,
    "esModuleInterop": true,
    "module": "esnext",
    "moduleResolution": "bundler",
    "resolveJsonModule": true,
    "isolatedModules": true,
    "jsx": "preserve",
    "incremental": true,
    "plugins": [
      {
        "name": "next"
      }
    ],
    "paths": {
      "@/*": ["./src/*"]
    }
  },
  "include": ["next-env.d.ts", "**/*.ts", "**/*.tsx", ".next/types/**/*.ts"],
  "exclude": ["node_modules"]
}
```

---

### Task 8.3: Create Frontend README

**File**: `src/frontend/README.md`

```markdown
# MythAPI Frontend

A Next.js 14+ frontend application for the MythAPI backend, providing a modern web interface for exploring and managing gods and mythologies.

## Features

- **Browse Gods**: View a comprehensive list of gods with search functionality
- **God Details**: Detailed information about each god, including aliases
- **CRUD Operations**: Create, update, and view gods
- **Mythologies**: Explore different mythologies and their associated gods
- **Search**: Real-time search with alias support
- **Responsive Design**: Mobile-friendly interface

## Tech Stack

- **Framework**: Next.js 14+ (App Router)
- **Language**: TypeScript
- **Styling**: Tailwind CSS
- **HTTP Client**: Axios
- **Validation**: Zod
- **Notifications**: React Hot Toast
- **Icons**: Heroicons

## Getting Started

### Prerequisites

- Node.js 18+ or 20+
- npm or yarn
- MythAPI backend running on port 5280

### Installation

```bash
# Install dependencies
npm install

# Copy environment variables
cp .env.example .env.local

# Update .env.local with your API URL
```

### Development

```bash
# Run development server
npm run dev

# Open http://localhost:3000
```

### Build

```bash
# Build for production
npm run build

# Start production server
npm start
```

## Project Structure

```
src/frontend/
├── src/
│   ├── app/              # Next.js App Router pages
│   ├── components/       # Reusable React components
│   ├── lib/              # Utilities, API clients, types
│   └── styles/           # Global styles
├── public/               # Static assets
└── package.json
```

## API Integration

The frontend communicates with the MythAPI backend through a centralized API client located in `src/lib/api/`. All API calls are type-safe and include error handling.

### Environment Variables

- `NEXT_PUBLIC_API_BASE_URL`: Backend API base URL (default: http://localhost:5280)
- `NEXT_PUBLIC_API_VERSION`: API version (default: v1)

## Key Components

### Pages

- `/` - Home page with feature highlights
- `/gods` - List all gods with search
- `/gods/[id]` - God detail view
- `/gods/new` - Create new god
- `/gods/[id]/edit` - Edit existing god
- `/mythologies` - List all mythologies

### Components

- **GodCard** - Display god information in a card
- **GodForm** - Form for creating/editing gods
- **SearchBar** - Search input with debouncing
- **Button** - Reusable button component
- **Card** - Card container with variants

## Development Guidelines

### Component Patterns

- Use **Server Components** by default for better performance
- Add `'use client'` directive only when needed (forms, search, interactive elements)
- Keep components small and focused on a single responsibility

### State Management

- Use React Server Components for data fetching
- Use client-side state for interactive features
- Leverage Next.js router for navigation and data refresh

### Error Handling

- All API calls include try-catch blocks
- User-friendly error messages with toast notifications
- Error boundaries for page-level errors

### Testing

```bash
# Run tests (to be implemented)
npm test

# Run tests in watch mode
npm test:watch
```

## Contributing

1. Create a feature branch
2. Make your changes
3. Test thoroughly
4. Submit a pull request

## License

MIT
```

---

### Task 8.4: Create Package.json Scripts

**File**: `src/frontend/package.json` (key scripts section)

```json
{
  "scripts": {
    "dev": "next dev",
    "build": "next build",
    "start": "next start",
    "lint": "next lint",
    "type-check": "tsc --noEmit",
    "format": "prettier --write \"src/**/*.{ts,tsx,js,jsx,json,css,md}\"",
    "format:check": "prettier --check \"src/**/*.{ts,tsx,js,jsx,json,css,md}\""
  }
}
```

---

## Phase 9: Testing and Quality Assurance

### Task 9.1: Manual Testing Checklist

**Test Cases**:

#### Gods List Page
- [ ] Page loads without errors
- [ ] All gods are displayed in a grid
- [ ] Search bar is visible and functional
- [ ] Searching updates the grid
- [ ] Clear button works
- [ ] "Add God" button navigates to create page
- [ ] Empty state shows when no gods exist
- [ ] Loading state displays while fetching

#### God Detail Page
- [ ] Correct god information is displayed
- [ ] Back button navigates to list
- [ ] Edit button navigates to edit page
- [ ] Aliases are displayed correctly
- [ ] 404 page shows for invalid IDs
- [ ] Loading state displays while fetching

#### Create God Page
- [ ] Form is displayed
- [ ] All fields are present (name, description, mythology)
- [ ] Mythology dropdown is populated
- [ ] Validation errors display correctly
- [ ] Success creates god and redirects
- [ ] Cancel button works (if implemented)
- [ ] Toast notification on success

#### Edit God Page
- [ ] Form is pre-filled with god data
- [ ] All fields are editable
- [ ] Mythology dropdown shows current selection
- [ ] Validation errors display correctly
- [ ] Success updates god and redirects
- [ ] Toast notification on success

#### Mythologies Page
- [ ] All mythologies are displayed
- [ ] God count is accurate
- [ ] Featured gods are shown
- [ ] Empty state displays when no mythologies exist

#### General
- [ ] Navigation works between all pages
- [ ] Page titles are correct
- [ ] Toast notifications work
- [ ] Error pages display correctly
- [ ] Loading states work
- [ ] Responsive design on mobile
- [ ] Responsive design on tablet
- [ ] Responsive design on desktop

---

### Task 9.2: Browser Compatibility Testing

**Test in**:
- [ ] Chrome (latest)
- [ ] Firefox (latest)
- [ ] Safari (latest)
- [ ] Edge (latest)
- [ ] Mobile Safari (iOS)
- [ ] Mobile Chrome (Android)

---

### Task 9.3: Performance Testing

**Metrics to check**:
- [ ] Initial page load < 2s
- [ ] Search debounce working (not calling API on every keystroke)
- [ ] Images optimized (if any)
- [ ] No console errors
- [ ] Lighthouse score > 90

---

## Phase 10: Deployment Preparation

### Task 10.1: Environment Configuration for Production

**File**: `src/frontend/.env.production`

```env
NEXT_PUBLIC_API_BASE_URL=https://your-production-api.com
NEXT_PUBLIC_API_VERSION=v1
```

---

### Task 10.2: Docker Configuration (Optional)

**File**: `src/frontend/Dockerfile`

```dockerfile
# Multi-stage build for Next.js

# Stage 1: Dependencies
FROM node:20-alpine AS deps
WORKDIR /app
COPY package.json package-lock.json ./
RUN npm ci

# Stage 2: Build
FROM node:20-alpine AS builder
WORKDIR /app
COPY --from=deps /app/node_modules ./node_modules
COPY . .
RUN npm run build

# Stage 3: Runner
FROM node:20-alpine AS runner
WORKDIR /app

ENV NODE_ENV production

RUN addgroup --system --gid 1001 nodejs
RUN adduser --system --uid 1001 nextjs

COPY --from=builder /app/public ./public
COPY --from=builder --chown=nextjs:nodejs /app/.next/standalone ./
COPY --from=builder --chown=nextjs:nodejs /app/.next/static ./.next/static

USER nextjs

EXPOSE 3000

ENV PORT 3000

CMD ["node", "server.js"]
```

**File**: `src/frontend/.dockerignore`

```
node_modules
.next
.git
.gitignore
README.md
.env.local
npm-debug.log
```

---

### Task 10.3: Update Next.js Config for Docker

**File**: `src/frontend/next.config.js` (add output config)

```javascript
/** @type {import('next').NextConfig} */
const nextConfig = {
  reactStrictMode: true,
  output: 'standalone', // For Docker builds
  
  async rewrites() {
    return [
      {
        source: '/api/:path*',
        destination: 'http://localhost:5280/api/:path*',
      },
    ];
  },
  
  images: {
    domains: [],
  },
};

module.exports = nextConfig;
```

---

## Phase 11: Additional Enhancements (Optional)

### Task 11.1: Add Pagination

**Location**: `src/frontend/src/app/gods/page.tsx`

**Implementation**:
- Add state for current page and page size
- Implement pagination controls component
- Slice gods array based on current page
- Add page navigation buttons

---

### Task 11.2: Add Filtering

**Features**:
- Filter by mythology
- Filter by name length
- Multiple filter combinations

---

### Task 11.3: Add Sorting

**Features**:
- Sort by name (A-Z, Z-A)
- Sort by ID
- Sort by mythology

---

### Task 11.4: Add Dark Mode

**Implementation**:
- Add theme context
- Update Tailwind config
- Add theme toggle button
- Store preference in localStorage

---

## Dependencies Summary

### Required NPM Packages

```json
{
  "dependencies": {
    "next": "^14.0.0",
    "react": "^18.0.0",
    "react-dom": "^18.0.0",
    "axios": "^1.6.0",
    "react-hot-toast": "^2.4.1",
    "@heroicons/react": "^2.1.0",
    "clsx": "^2.0.0",
    "zod": "^3.22.0"
  },
  "devDependencies": {
    "@types/node": "^20.0.0",
    "@types/react": "^18.0.0",
    "@types/react-dom": "^18.0.0",
    "typescript": "^5.0.0",
    "tailwindcss": "^3.4.0",
    "postcss": "^8.0.0",
    "autoprefixer": "^10.0.0",
    "eslint": "^8.0.0",
    "eslint-config-next": "^14.0.0"
  }
}
```

---

## Implementation Timeline

### Week 1: Foundation
- **Day 1-2**: Project setup, configuration, type definitions
- **Day 3-4**: API client and service functions
- **Day 5**: Utility functions and validators

### Week 2: Components
- **Day 1-2**: UI components (Button, Card, Loading, SearchBar)
- **Day 3-4**: God-specific components (GodCard, GodForm, GodsList)
- **Day 5**: Layout components (Header, Footer)

### Week 3: Pages
- **Day 1**: Root layout and home page
- **Day 2**: Gods list and detail pages
- **Day 3**: Create and edit pages
- **Day 4**: Mythologies page
- **Day 5**: Loading and error states

### Week 4: Testing and Polish
- **Day 1-2**: Manual testing and bug fixes
- **Day 3**: Performance optimization
- **Day 4**: Documentation
- **Day 5**: Deployment preparation

---

## Risk Mitigation

### Potential Issues and Solutions

| Risk | Impact | Mitigation |
|------|--------|------------|
| API endpoint changes | High | Use typed API client, centralize API calls |
| CORS issues | High | Configure backend CORS properly, use proxy in dev |
| Type mismatches | Medium | Strict TypeScript configuration, validate at boundaries |
| Performance issues | Medium | Use Server Components, optimize images, lazy load |
| State management complexity | Low | Keep state minimal, use URL state where possible |
| Browser compatibility | Low | Test in multiple browsers, use standard web APIs |

---

## Success Criteria

### Must Have
- ✅ All CRUD operations working for Gods
- ✅ Search functionality with debouncing
- ✅ Responsive design (mobile, tablet, desktop)
- ✅ Error handling and user feedback
- ✅ Loading states for async operations
- ✅ Type-safe API integration
- ✅ Mythologies display

### Should Have
- ✅ Form validation with clear error messages
- ✅ Toast notifications
- ✅ Clean, modern UI
- ✅ Accessibility (ARIA labels, keyboard navigation)
- ✅ SEO-friendly (meta tags, semantic HTML)

### Nice to Have
- ⭕ Pagination
- ⭕ Filtering and sorting
- ⭕ Dark mode
- ⭕ Unit tests
- ⭕ E2E tests
- ⭕ Animations and transitions

---

## Conclusion

This implementation plan provides a comprehensive roadmap for building a production-ready Next.js frontend for the MythAPI. The architecture follows Next.js 14+ best practices with the App Router, server and client components, and proper TypeScript typing throughout.

The modular component structure ensures maintainability and reusability, while the centralized API client provides a single source of truth for backend communication. Error handling, loading states, and user feedback mechanisms ensure a smooth user experience.

Follow the phases sequentially for the best results, and refer to the testing checklist to ensure all functionality works as expected before deployment.

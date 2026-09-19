export interface DocumentDto {
  id: string;
  title: string;
  fileName: string;
  status: string;
  totalChunks: number;
  createdAt: string;
  processedAt?: string;
}

export interface ReviewItemDto {
  progressId: string;
  itemId: string;
  documentId: string;
  documentTitle: string;
  type: string;
  topic: string;
  question: string;
  answer: string;
  options?: string;
}

export interface BalanceDto {
  tokenBalance: number;
  planId?: string;
  planName?: string;
}

export interface DashboardSummaryDto {
  dueToday: number;
  reviewedItems: number;
  totalItems: number;
  streakDays: number;
  reviewsLast7Days: number[];
  documentProgress: {
    id: string;
    documentTitle: string;
    progress: number;
    totalItems: number;
    reviewedItems: number;
  }[];
}

export interface PlanDto {
  id: string;
  name: string;
  price: number;
  priceDisplay: string;
  tokens: number;
  monthlyTokens: number;
}
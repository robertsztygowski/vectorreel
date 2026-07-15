import { Hero } from '@/components/landing/Hero';
import { TrustStrip } from '@/components/landing/TrustStrip';
import { Steps } from '@/components/landing/Steps';
import { FeatureCards } from '@/components/landing/FeatureCards';
import { EuBand } from '@/components/landing/EuBand';
import { FinalCta } from '@/components/landing/FinalCta';

export default function LandingPage() {
  return (
    <>
      <Hero />
      <TrustStrip />
      <Steps />
      <FeatureCards />
      <EuBand />
      <FinalCta />
    </>
  );
}

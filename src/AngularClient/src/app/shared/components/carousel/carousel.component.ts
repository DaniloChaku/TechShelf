import { Component, Input } from '@angular/core';
import { FaIconComponent } from '@fortawesome/angular-fontawesome';
import {
  faCircleArrowRight,
  faCircleArrowLeft,
} from '@fortawesome/free-solid-svg-icons';

@Component({
  selector: 'app-carousel',
  standalone: true,
  imports: [FaIconComponent],
  templateUrl: './carousel.component.html',
  styleUrl: './carousel.component.css',
})
export class CarouselComponent {
  @Input() items: string[] = [];
  currentSlide = 0;
  faArrowRight = faCircleArrowRight;
  faArrowLeft = faCircleArrowLeft;

  prev() {
    this.currentSlide =
      this.currentSlide === 0
        ? this.items.length - 1
        : this.currentSlide - 1;
  }

  next() {
    this.currentSlide =
      this.currentSlide === this.items.length - 1
        ? 0
        : this.currentSlide + 1;
  }

  goToSlide(index: number) {
    this.currentSlide = index;
  }
}
